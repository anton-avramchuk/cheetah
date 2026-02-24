using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Cheetah.Backend.Jwt.Abstractions;
using Cheetah.Backend.Jwt.Options;
using Cheetah.Core.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Cheetah.Backend.Jwt.Services;

[Export(LifetimeType.Singleton, typeof(IJwtTokenGenerator))]
public class JwtTokenGenerator(IOptions<JwtOptions> options) : IJwtTokenGenerator
{
    private static readonly JwtSecurityTokenHandler _tokenHandler = new();
    private readonly JwtOptions _options = options.Value;

    public TokenGenerationResult GenerateToken(
        Guid userId,
        string userName,
        string email,
        IEnumerable<string> roles,
        IEnumerable<Claim>? additionalClaims = null)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, userName),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        if (additionalClaims is not null)
            claims.AddRange(additionalClaims);

        var expiresInSeconds = _options.ExpirationMinutes * 60;

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(expiresInSeconds),
            signingCredentials: credentials);

        return new TokenGenerationResult(_tokenHandler.WriteToken(token), expiresInSeconds);
    }
}
