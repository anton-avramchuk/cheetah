using Cheetah.Core.Events;

namespace Cheetah.Backend.IdentityCore.Events;

/// <summary>
/// Event raised when a role is removed from a user
/// </summary>
public record UserRoleRemovedEvent(
    Guid UserId,
    Guid RoleId
) : EventBase;
