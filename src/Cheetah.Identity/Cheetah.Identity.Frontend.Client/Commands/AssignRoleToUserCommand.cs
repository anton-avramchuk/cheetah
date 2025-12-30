using Cheetah.Core.CQRS;

namespace Cheetah.Identity.Frontend.Client.Commands;

public record AssignRoleToUserCommand(
    Guid UserId,
    Guid RoleId
) : ICommand;
