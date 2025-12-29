using Cheetah.Core.Events;

namespace Cheetah.Tenants.Events;

/// <summary>
/// Event raised when a tenant is activated
/// </summary>
public record TenantActivatedEvent(
    Guid TenantId,
    DateTime ActivatedAt
) : EventBase;
