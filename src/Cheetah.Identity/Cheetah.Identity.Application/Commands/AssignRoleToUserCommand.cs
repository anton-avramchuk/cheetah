using Cheetah.Core.CQRS;

namespace Cheetah.Identity.Application.Commands;

/// <summary>
/// Command to assign a role to a user
/// </summary>
public record AssignRoleToUserCommand(
    Guid UserId,
    Guid RoleId
) : ICommand;
