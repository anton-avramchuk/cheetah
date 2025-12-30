using Cheetah.Core.Events;

namespace Cheetah.Identity.Events;

/// <summary>
/// Event raised when a new role is created
/// </summary>
public record RoleCreatedEvent(
    Guid RoleId,
    string RoleName,
    DateTime CreatedAt
) : EventBase;
