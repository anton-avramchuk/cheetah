using Crm.Identity.Application.Commands;

namespace Crm.Identity.Application.Services;

public interface ITokenGenerator
{
    TokenResult GenerateToken(Guid userId, string userName, string email, IEnumerable<string> roles);
}
