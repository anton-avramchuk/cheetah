using System.Security.Claims;

namespace Cheetah.Backend.Jwt.Abstractions;

public interface IJwtTokenGenerator
{
    TokenGenerationResult GenerateToken(
        Guid userId,
        string userName,
        string email,
        IEnumerable<string> roles,
        IEnumerable<Claim>? additionalClaims = null);
}
