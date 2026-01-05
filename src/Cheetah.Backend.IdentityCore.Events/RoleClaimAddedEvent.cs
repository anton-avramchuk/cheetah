using Cheetah.Core.Events;

namespace Cheetah.Backend.IdentityCore.Events;

/// <summary>
/// Event raised when a claim is added to a role
/// </summary>
public record RoleClaimAddedEvent(
    Guid RoleId,
    string ClaimType,
    string ClaimValue
) : EventBase;
