using Cheetah.Core.Events;

namespace Cheetah.Backend.IdentityCore.Events;

/// <summary>
/// Event raised when a user is unlocked
/// </summary>
public record UserUnlockedEvent(
    Guid UserId
) : EventBase;
