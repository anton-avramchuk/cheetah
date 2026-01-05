using Cheetah.Core.Events;

namespace Cheetah.Backend.IdentityCore.Events;

/// <summary>
/// Event raised when a role's name is changed
/// </summary>
public record RoleNameChangedEvent(
    Guid RoleId,
    string NewName,
    string OldName
) : EventBase;
