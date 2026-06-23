using System.Security.Claims;
using Cheetah.Backend.Jwt.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.Application.Services;

namespace Cheetah.Modules.Identity.Api;

[Export(LifetimeType.Singleton, typeof(ITokenGenerator))]
internal sealed class JwtTokenGeneratorBridge(IJwtTokenGenerator inner) : ITokenGenerator
{
    public TokenResult GenerateToken(Guid userId, string userName, string email, IEnumerable<string> roles,
        IEnumerable<Claim> claims)
    {
        var result = inner.GenerateToken(userId, userName, email, roles, claims);
        return new TokenResult(result.Token, result.ExpiresInSeconds);
    }
}