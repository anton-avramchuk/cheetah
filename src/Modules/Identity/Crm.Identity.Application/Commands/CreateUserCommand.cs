using Cheetah.Core.CQRS;

namespace Crm.Identity.Application.Commands;

public record CreateUserCommand(string UserName, string Email, string Password, IReadOnlyList<Guid>? RoleIds = null) : ICommand<Guid>;
