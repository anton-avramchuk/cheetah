using Cheetah.Core.CQRS;

namespace Cheetah.Identity.Application.Commands;

/// <summary>
/// Command to add a personal permission (claim) to a user
/// </summary>
public record AddPermissionToUserCommand(
    Guid UserId,
    string Permission
) : ICommand;
