using Cheetah.Core.Events;

namespace Cheetah.Backend.IdentityCore.Events;

/// <summary>
/// Event raised when a user is locked out
/// </summary>
public record UserLockedOutEvent(
    Guid UserId,
    DateTimeOffset LockoutEnd
) : EventBase;
