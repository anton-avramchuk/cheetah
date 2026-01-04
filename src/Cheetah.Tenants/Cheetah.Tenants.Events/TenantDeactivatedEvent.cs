using Cheetah.Core.Tenants.Events;

namespace Cheetah.Tenants.Events;

/// <summary>
/// Event raised when a tenant is deactivated
/// </summary>
public record TenantDeactivatedEvent(
    Guid TenantId,
    DateTime DeactivatedAt
) : Core.Tenants.Events.TenantDeactivatedEvent(TenantId);
