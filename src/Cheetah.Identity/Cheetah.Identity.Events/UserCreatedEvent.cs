using Cheetah.Core.Events;

namespace Cheetah.Identity.Events;

/// <summary>
/// Event raised when a new user is created
/// </summary>
public record UserCreatedEvent(
    Guid UserId,
    string Email,
    string? FirstName,
    string? LastName,
    DateTime CreatedAt
) : EventBase;
