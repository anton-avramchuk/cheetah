using System.Security.Cryptography;
using Cheetah.Backend.Jwt.Options;
using Shouldly;

namespace Cheetah.Backend.Jwt.Tests;

public class JwtOptionsValidatorTests
{
    private static readonly JwtOptionsValidator Validator = new();

    private static JwtOptions Base() => new() { Issuer = "cheetah", Audience = "cheetah-clients" };

    [Fact]
    public void Hs256_WithShortSecret_Fails()
    {
        var opts = Base();
        opts.SigningAlgorithm = JwtSigningAlgorithms.Hs256;
        opts.SecretKey = "too-short";

        Validator.Validate(null, opts).Failed.ShouldBeTrue();
    }

    [Fact]
    public void Rs256_WithoutKey_AndNoMetadata_Fails()
    {
        var opts = Base();
        opts.SigningAlgorithm = JwtSigningAlgorithms.Rs256;

        Validator.Validate(null, opts).Failed.ShouldBeTrue();
    }

    [Fact]
    public void Rs256_ValidatorOnly_WithMetadata_Succeeds()
    {
        var opts = Base();
        opts.SigningAlgorithm = JwtSigningAlgorithms.Rs256;
        opts.MetadataAddress = "https://identity.cheetah.io/.well-known/openid-configuration";

        Validator.Validate(null, opts).Succeeded.ShouldBeTrue();
    }

    [Fact]
    public void Metadata_WithBadUri_Fails()
    {
        var opts = Base();
        opts.MetadataAddress = "not-a-uri";

        Validator.Validate(null, opts).Failed.ShouldBeTrue();
    }

    [Fact]
    public void Rs256_Issuer_WithPrivateKey_Succeeds()
    {
        using var rsa = RSA.Create(2048);
        var opts = Base();
        opts.SigningAlgorithm = JwtSigningAlgorithms.Rs256;
        opts.PrivateKeyPem = rsa.ExportPkcs8PrivateKeyPem();

        Validator.Validate(null, opts).Succeeded.ShouldBeTrue();
    }
}
