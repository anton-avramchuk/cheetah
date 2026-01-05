using Cheetah.Core.Events;

namespace Cheetah.Backend.IdentityCore.Events;

/// <summary>
/// Event raised when a new role is created
/// </summary>
public record RoleCreatedEvent(
    Guid RoleId,
    string Name
) : EventBase;
