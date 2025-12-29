using Cheetah.Core.Events;

namespace Cheetah.Identity.Events;

/// <summary>
/// Event raised when a user successfully logs in
/// </summary>
public record UserLoggedInEvent(
    Guid UserId,
    string Email,
    DateTime LoggedInAt
) : EventBase;
