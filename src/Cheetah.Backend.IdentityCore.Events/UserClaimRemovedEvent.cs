using Cheetah.Core.Events;

namespace Cheetah.Backend.IdentityCore.Events;

/// <summary>
/// Event raised when a claim is removed from a user
/// </summary>
public record UserClaimRemovedEvent(
    Guid UserId,
    string ClaimType,
    string ClaimValue
) : EventBase;
