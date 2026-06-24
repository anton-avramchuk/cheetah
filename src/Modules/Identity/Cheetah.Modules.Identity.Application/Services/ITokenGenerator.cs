using System.Security.Claims;
using Cheetah.Modules.Identity.Application.Commands;

namespace Cheetah.Modules.Identity.Application.Services;

public interface ITokenGenerator
{
    TokenResult GenerateToken(Guid userId, string userName, string email, IEnumerable<string> roles, IEnumerable<Claim> claims);

    /// <summary>Выпускает сервисный (machine-to-machine) токен для вызывающего сервиса.</summary>
    TokenResult GenerateServiceToken(string clientId, IEnumerable<string> roles);
}
