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

    /// <summary>
    /// Выпускает короткоживущий сервисный токен (machine-to-machine) для вызывающего сервиса.
    /// Помечается claim'ом <c>token_type=service</c>, <c>sub</c> = <paramref name="clientId"/>.
    /// </summary>
    TokenGenerationResult GenerateServiceToken(
        string clientId,
        IEnumerable<string> roles,
        IEnumerable<Claim>? additionalClaims = null);
}
