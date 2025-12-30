using Cheetah.Core.Events;

namespace Cheetah.Identity.Events;

/// <summary>
/// Event raised when a role is assigned to a user
/// </summary>
public record UserRoleAssignedEvent(
    Guid UserId,
    string UserEmail,
    Guid RoleId,
    string RoleName,
    DateTime AssignedAt
) : EventBase;
