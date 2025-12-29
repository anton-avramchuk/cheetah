using Cheetah.Core.Events;

namespace Cheetah.Tenants.Events;

/// <summary>
/// Event raised when a tenant is deactivated
/// </summary>
public record TenantDeactivatedEvent(
    Guid TenantId,
    DateTime DeactivatedAt
) : EventBase;
