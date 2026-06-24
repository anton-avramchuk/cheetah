using Microsoft.Extensions.Options;

namespace Cheetah.Backend.Jwt.Options;

/// <summary>
/// Условная валидация: требования к ключам зависят от алгоритма и режима (локальный/JWKS).
/// </summary>
public sealed class JwtOptionsValidator : IValidateOptions<JwtOptions>
{
    public ValidateOptionsResult Validate(string? name, JwtOptions options)
    {
        var usesMetadata = !string.IsNullOrWhiteSpace(options.MetadataAddress);

        if (usesMetadata && !Uri.TryCreate(options.MetadataAddress, UriKind.Absolute, out _))
            return ValidateOptionsResult.Fail("Jwt:MetadataAddress must be a valid absolute URI");

        var isRs256 = string.Equals(options.SigningAlgorithm, JwtSigningAlgorithms.Rs256, StringComparison.OrdinalIgnoreCase);
        var isHs256 = string.Equals(options.SigningAlgorithm, JwtSigningAlgorithms.Hs256, StringComparison.OrdinalIgnoreCase);

        if (!isRs256 && !isHs256)
            return ValidateOptionsResult.Fail($"Jwt:SigningAlgorithm must be '{JwtSigningAlgorithms.Hs256}' or '{JwtSigningAlgorithms.Rs256}'");

        // Сервис-валидатор (MetadataAddress) ключей в конфиге не держит.
        if (usesMetadata)
            return ValidateOptionsResult.Success;

        if (isHs256 && (string.IsNullOrEmpty(options.SecretKey) || options.SecretKey.Length < 32))
            return ValidateOptionsResult.Fail("Jwt:SecretKey is required and must be at least 32 characters for HS256");

        if (isRs256 && string.IsNullOrWhiteSpace(options.PrivateKeyPem))
            return ValidateOptionsResult.Fail("Jwt:PrivateKeyPem is required for RS256 (issuer/local validation)");

        return ValidateOptionsResult.Success;
    }
}
