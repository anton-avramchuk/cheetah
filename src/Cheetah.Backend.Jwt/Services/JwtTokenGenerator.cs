using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Cheetah.Backend.Jwt.Abstractions;
using Cheetah.Backend.Jwt.Options;
using Cheetah.Core.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Cheetah.Backend.Jwt.Services;

[Export(LifetimeType.Singleton, typeof(IJwtTokenGenerator))]
public class JwtTokenGenerator(IOptions<JwtOptions> options, IJwtSigningKeyProvider signingKeyProvider)
    : IJwtTokenGenerator
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
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, userName),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        claims.AddRange(roles.Select(role => new Claim("role", role)));

        if (additionalClaims is not null)
            claims.AddRange(additionalClaims);

        return BuildToken(claims, _options.ExpirationMinutes * 60);
    }

    public TokenGenerationResult GenerateServiceToken(
        string clientId,
        IEnumerable<string> roles,
        IEnumerable<Claim>? additionalClaims = null)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, clientId),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("client_id", clientId),
            new("token_type", "service"),
        };

        claims.AddRange(roles.Select(role => new Claim("role", role)));

        if (additionalClaims is not null)
            claims.AddRange(additionalClaims);

        return BuildToken(claims, _options.ServiceTokenExpirationMinutes * 60);
    }

    private TokenGenerationResult BuildToken(IEnumerable<Claim> claims, int expiresInSeconds)
    {
        var credentials = signingKeyProvider.GetSigningCredentials();

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(expiresInSeconds),
            signingCredentials: credentials);

        return new TokenGenerationResult(_tokenHandler.WriteToken(token), expiresInSeconds);
    }
}
