using Cheetah.Backend.Jwt.Abstractions;
using Cheetah.Core.DependencyInjection;
using Crm.Identity.Application.Commands;
using Crm.Identity.Application.Services;

namespace Crm.Identity.Api;

[Export(LifetimeType.Singleton, typeof(ITokenGenerator))]
internal sealed class JwtTokenGeneratorBridge(IJwtTokenGenerator inner) : ITokenGenerator
{
    public TokenResult GenerateToken(Guid userId, string userName, string email, IEnumerable<string> roles)
    {
        var result = inner.GenerateToken(userId, userName, email, roles);
        return new TokenResult(result.Token, result.ExpiresInSeconds);
    }
}
