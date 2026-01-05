using Cheetah.Core.Events;

namespace Cheetah.Backend.IdentityCore.Events;

/// <summary>
/// Event raised when a new user is created
/// </summary>
public record UserCreatedEvent(
    Guid UserId,
    string UserName,
    string Email
) : EventBase;
