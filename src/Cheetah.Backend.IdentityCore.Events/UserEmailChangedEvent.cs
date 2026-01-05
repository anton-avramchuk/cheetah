using Cheetah.Core.Events;

namespace Cheetah.Backend.IdentityCore.Events;

/// <summary>
/// Event raised when a user's email is changed
/// </summary>
public record UserEmailChangedEvent(
    Guid UserId,
    string NewEmail,
    string OldEmail
) : EventBase;
