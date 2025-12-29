using Cheetah.Core.Events;

namespace Cheetah.Identity.Events;

/// <summary>
/// Event raised when a user confirms their email address
/// </summary>
public record EmailConfirmedEvent(
    Guid UserId,
    string Email,
    DateTime ConfirmedAt
) : EventBase;
