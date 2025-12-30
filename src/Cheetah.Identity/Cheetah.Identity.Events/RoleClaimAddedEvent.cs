using Cheetah.Core.Events;

namespace Cheetah.Identity.Events;

/// <summary>
/// Event raised when a claim (permission) is added to a role
/// Used for audit trail
/// </summary>
public record RoleClaimAddedEvent(
    Guid RoleId,
    string RoleName,
    string ClaimType,
    string ClaimValue,
    DateTime AddedAt
) : EventBase;
