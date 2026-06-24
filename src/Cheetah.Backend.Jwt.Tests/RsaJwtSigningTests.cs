using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Cheetah.Backend.Jwt.Options;
using Cheetah.Backend.Jwt.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shouldly;
using MsOptions = Microsoft.Extensions.Options.Options;

namespace Cheetah.Backend.Jwt.Tests;

public class RsaJwtSigningTests
{
    private static IOptions<JwtOptions> RsaOptions(out string pem)
    {
        using var rsa = RSA.Create(2048);
        pem = rsa.ExportPkcs8PrivateKeyPem();
        return MsOptions.Create(new JwtOptions
        {
            SigningAlgorithm = JwtSigningAlgorithms.Rs256,
            PrivateKeyPem = pem,
            Issuer = "cheetah",
            Audience = "cheetah-clients",
        });
    }

    [Fact]
    public void Provider_IsAsymmetric_AndPublishesSingleJwk()
    {
        var provider = new RsaJwtSigningKeyProvider(RsaOptions(out _));

        provider.IsAsymmetric.ShouldBeTrue();

        var keys = provider.GetPublicWebKeys();
        keys.Count.ShouldBe(1);
        keys[0].Kty.ShouldBe("RSA");
        keys[0].Use.ShouldBe("sig");
        keys[0].Alg.ShouldBe(SecurityAlgorithms.RsaSha256);
        keys[0].N.ShouldNotBeNullOrEmpty();
        keys[0].E.ShouldNotBeNullOrEmpty();
        keys[0].Kid.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void GeneratedToken_IsRs256_WithKid_AndValidatesAgainstPublicKey()
    {
        var provider = new RsaJwtSigningKeyProvider(RsaOptions(out _));
        var options = RsaOptions(out _); // separate key — only the provider's key is used below
        var generator = new JwtTokenGenerator(
            MsOptions.Create(new JwtOptions { Issuer = "cheetah", Audience = "cheetah-clients" }),
            provider);

        var result = generator.GenerateToken(Guid.NewGuid(), "alice", "alice@cheetah.io", new[] { "Admin" });

        var handler = new JwtSecurityTokenHandler();
        handler.ValidateToken(result.Token, new TokenValidationParameters
        {
            ValidIssuer = "cheetah",
            ValidAudience = "cheetah-clients",
            IssuerSigningKey = provider.GetValidationKey(),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
        }, out var validated);

        var jwt = (JwtSecurityToken)validated;
        jwt.Header.Alg.ShouldBe(SecurityAlgorithms.RsaSha256);
        jwt.Header.Kid.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void TokenKid_MatchesPublishedJwkKid()
    {
        var provider = new RsaJwtSigningKeyProvider(RsaOptions(out _));
        var generator = new JwtTokenGenerator(
            MsOptions.Create(new JwtOptions { Issuer = "cheetah", Audience = "cheetah-clients" }),
            provider);

        var token = new JwtSecurityTokenHandler().ReadJwtToken(
            generator.GenerateServiceToken("svc-deals", new[] { "ServiceAccount" }).Token);

        token.Header.Kid.ShouldBe(provider.GetPublicWebKeys()[0].Kid);
        token.Claims.ShouldContain(c => c.Type == "token_type" && c.Value == "service");
    }

    [Fact]
    public void ExplicitKeyId_IsUsed()
    {
        using var rsa = RSA.Create(2048);
        var provider = new RsaJwtSigningKeyProvider(MsOptions.Create(new JwtOptions
        {
            SigningAlgorithm = JwtSigningAlgorithms.Rs256,
            PrivateKeyPem = rsa.ExportPkcs8PrivateKeyPem(),
            KeyId = "my-key-1",
            Issuer = "cheetah",
            Audience = "cheetah-clients",
        }));

        provider.GetPublicWebKeys()[0].Kid.ShouldBe("my-key-1");
    }
}
