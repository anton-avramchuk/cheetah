using Cheetah.Core.Events;

namespace Cheetah.Identity.Events;

/// <summary>
/// Event raised when a user changes their password
/// </summary>
public record PasswordChangedEvent(
    Guid UserId,
    DateTime ChangedAt
) : EventBase;
