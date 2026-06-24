using System.ComponentModel.DataAnnotations;

namespace Cheetah.Backend.Jwt.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>
    /// Алгоритм подписи: <c>HS256</c> (симметричный, default) или <c>RS256</c> (асимметричный).
    /// При RS256 выпускать токены может только владелец приватного ключа (Identity), остальные
    /// валидируют публичным ключом через JWKS.
    /// </summary>
    public string SigningAlgorithm { get; set; } = JwtSigningAlgorithms.Hs256;

    /// <summary>Секрет для HS256. Требуется (≥32 символа), когда алгоритм HS256 и нет <see cref="MetadataAddress"/>.</summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Приватный RSA-ключ (PEM) для подписи при RS256. Только у эмитента (Identity).
    /// Из него же выводится публичный ключ для валидации и JWKS.
    /// </summary>
    public string PrivateKeyPem { get; set; } = string.Empty;

    /// <summary>
    /// Идентификатор ключа (<c>kid</c>) в заголовке токена и в JWKS. Если пусто — вычисляется
    /// как стабильный отпечаток публичного ключа (нужно для подбора ключа при ротации).
    /// </summary>
    public string KeyId { get; set; } = string.Empty;

    /// <summary>
    /// URL OIDC-discovery (<c>/.well-known/openid-configuration</c>) эмитента. Если задан —
    /// сервис-валидатор берёт ключи по JWKS (микросервисный режим) и не нуждается в локальном ключе.
    /// </summary>
    public string MetadataAddress { get; set; } = string.Empty;

    /// <summary>Требовать HTTPS для <see cref="MetadataAddress"/>. Default = true; выключайте только локально.</summary>
    public bool RequireHttpsMetadata { get; set; } = true;

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    public int ExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// Время жизни сервисного (machine-to-machine) токена. Намеренно короткое —
    /// сервис обновляет токен по мере истечения.
    /// </summary>
    public int ServiceTokenExpirationMinutes { get; set; } = 10;
}

public static class JwtSigningAlgorithms
{
    public const string Hs256 = "HS256";
    public const string Rs256 = "RS256";
}
