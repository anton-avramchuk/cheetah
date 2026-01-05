using Cheetah.Core.Events;

namespace Cheetah.Backend.IdentityCore.Events;

/// <summary>
/// Event raised when a claim is added to a user
/// </summary>
public record UserClaimAddedEvent(
    Guid UserId,
    string ClaimType,
    string ClaimValue
) : EventBase;
