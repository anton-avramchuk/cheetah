using Cheetah.Core.Events;

namespace Cheetah.Backend.IdentityCore.Events;

/// <summary>
/// Event raised when a role is added to a user
/// </summary>
public record UserRoleAddedEvent(
    Guid UserId,
    Guid RoleId
) : EventBase;
