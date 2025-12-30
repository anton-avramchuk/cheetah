using Cheetah.Core.CQRS;

namespace Cheetah.Identity.Application.Commands;

/// <summary>
/// Command to add a permission (claim) to a role
/// </summary>
public record AddPermissionToRoleCommand(
    Guid RoleId,
    string Permission
) : ICommand;
