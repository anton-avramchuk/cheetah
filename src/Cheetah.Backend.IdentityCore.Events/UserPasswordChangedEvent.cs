using Cheetah.Core.Events;

namespace Cheetah.Backend.IdentityCore.Events;

/// <summary>
/// Event raised when a user's password is changed
/// </summary>
public record UserPasswordChangedEvent(
    Guid UserId
) : EventBase;
