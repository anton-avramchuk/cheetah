using System.IdentityModel.Tokens.Jwt;
using Cheetah.Backend.Jwt.Options;
using Cheetah.Backend.Jwt.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shouldly;
using MsOptions = Microsoft.Extensions.Options.Options;

namespace Cheetah.Backend.Jwt.Tests;

public class HmacJwtSigningTests
{
    private static IOptions<JwtOptions> HmacOptions() => MsOptions.Create(new JwtOptions
    {
        SigningAlgorithm = JwtSigningAlgorithms.Hs256,
        SecretKey = new string('k', 40),
        Issuer = "cheetah",
        Audience = "cheetah-clients",
    });

    [Fact]
    public void Provider_IsSymmetric_AndPublishesNoJwks()
    {
        var provider = new HmacJwtSigningKeyProvider(HmacOptions());

        provider.IsAsymmetric.ShouldBeFalse();
        provider.GetPublicWebKeys().ShouldBeEmpty();
        provider.GetSigningCredentials().Algorithm.ShouldBe(SecurityAlgorithms.HmacSha256);
    }

    [Fact]
    public void GeneratedToken_IsHs256_AndValidates()
    {
        var options = HmacOptions();
        var provider = new HmacJwtSigningKeyProvider(options);
        var generator = new JwtTokenGenerator(options, provider);

        var result = generator.GenerateToken(Guid.NewGuid(), "bob", "bob@cheetah.io", new[] { "User" });

        var handler = new JwtSecurityTokenHandler();
        handler.ValidateToken(result.Token, new TokenValidationParameters
        {
            ValidIssuer = "cheetah",
            ValidAudience = "cheetah-clients",
            IssuerSigningKey = provider.GetValidationKey(),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
        }, out var validated);

        ((JwtSecurityToken)validated).Header.Alg.ShouldBe(SecurityAlgorithms.HmacSha256);
    }
}
