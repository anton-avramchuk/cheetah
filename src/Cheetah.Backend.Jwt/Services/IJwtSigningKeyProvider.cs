using Microsoft.IdentityModel.Tokens;

namespace Cheetah.Backend.Jwt.Services;

/// <summary>
/// Поставляет ключ подписи токенов и публичные ключи для JWKS. Реализация выбирается по
/// <c>Jwt:SigningAlgorithm</c>: HMAC (симметричный) или RSA (асимметричный, RS256).
/// </summary>
public interface IJwtSigningKeyProvider
{
    /// <summary>Учётные данные подписи для исходящих токенов.</summary>
    SigningCredentials GetSigningCredentials();

    /// <summary>Ключ для локальной валидации токенов в этом же процессе (без JWKS).</summary>
    SecurityKey GetValidationKey();

    /// <summary>true для RSA — тогда есть публичные ключи и имеет смысл публиковать JWKS.</summary>
    bool IsAsymmetric { get; }

    /// <summary>Публичные ключи для JWKS (<c>/.well-known/jwks.json</c>); пусто для HMAC.</summary>
    IReadOnlyList<JsonWebKey> GetPublicWebKeys();
}
