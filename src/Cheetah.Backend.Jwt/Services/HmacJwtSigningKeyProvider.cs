using System.Text;
using Cheetah.Backend.Jwt.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Cheetah.Backend.Jwt.Services;

/// <summary>Симметричная подпись (HS256) — поведение по умолчанию.</summary>
public sealed class HmacJwtSigningKeyProvider : IJwtSigningKeyProvider
{
    private readonly Lazy<SymmetricSecurityKey> _key;

    public HmacJwtSigningKeyProvider(IOptions<JwtOptions> options)
        => _key = new Lazy<SymmetricSecurityKey>(() =>
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Value.SecretKey)));

    public bool IsAsymmetric => false;

    public SigningCredentials GetSigningCredentials()
        => new(_key.Value, SecurityAlgorithms.HmacSha256);

    public SecurityKey GetValidationKey() => _key.Value;

    public IReadOnlyList<JsonWebKey> GetPublicWebKeys() => [];
}
