using System.Security.Cryptography;
using Cheetah.Backend.Jwt.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Cheetah.Backend.Jwt.Services;

/// <summary>
/// Асимметричная подпись (RS256). Приватный ключ держит только эмитент; публичный
/// отдаётся через JWKS. Ключ импортируется лениво, чтобы сервис-валидатор (режим
/// <c>MetadataAddress</c>) мог зависеть от провайдера, не имея приватного ключа.
/// </summary>
public sealed class RsaJwtSigningKeyProvider : IJwtSigningKeyProvider, IDisposable
{
    private readonly Lazy<KeyMaterial> _material;

    public RsaJwtSigningKeyProvider(IOptions<JwtOptions> options)
        => _material = new Lazy<KeyMaterial>(() => Load(options.Value));

    public bool IsAsymmetric => true;

    public SigningCredentials GetSigningCredentials()
        => new(_material.Value.PrivateKey, SecurityAlgorithms.RsaSha256);

    public SecurityKey GetValidationKey() => _material.Value.PublicKey;

    public IReadOnlyList<JsonWebKey> GetPublicWebKeys()
    {
        var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(_material.Value.PublicKey);
        jwk.Use = "sig";
        jwk.Alg = SecurityAlgorithms.RsaSha256;
        return [jwk];
    }

    private static KeyMaterial Load(JwtOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.PrivateKeyPem))
            throw new InvalidOperationException(
                "Jwt:PrivateKeyPem is required for RS256 signing. " +
                "Validator-only services must set Jwt:MetadataAddress instead.");

        var rsa = RSA.Create();
        rsa.ImportFromPem(options.PrivateKeyPem);

        var kid = string.IsNullOrWhiteSpace(options.KeyId)
            ? ComputeThumbprint(rsa)
            : options.KeyId;

        // Приватный ключ — для подписи; публичные параметры — для валидации/JWKS.
        var privateKey = new RsaSecurityKey(rsa) { KeyId = kid };
        var publicKey = new RsaSecurityKey(rsa.ExportParameters(includePrivateParameters: false)) { KeyId = kid };

        return new KeyMaterial(rsa, privateKey, publicKey);
    }

    private static string ComputeThumbprint(RSA rsa)
    {
        var spki = rsa.ExportSubjectPublicKeyInfo();
        var hash = SHA256.HashData(spki);
        return Base64UrlEncoder.Encode(hash);
    }

    public void Dispose()
    {
        if (_material.IsValueCreated)
            _material.Value.Rsa.Dispose();
    }

    private sealed record KeyMaterial(RSA Rsa, RsaSecurityKey PrivateKey, RsaSecurityKey PublicKey);
}
