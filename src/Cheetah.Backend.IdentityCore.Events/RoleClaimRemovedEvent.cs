using Cheetah.Core.Events;

namespace Cheetah.Backend.IdentityCore.Events;

/// <summary>
/// Event raised when a claim is removed from a role
/// </summary>
public record RoleClaimRemovedEvent(
    Guid RoleId,
    string ClaimType,
    string ClaimValue
) : EventBase;
