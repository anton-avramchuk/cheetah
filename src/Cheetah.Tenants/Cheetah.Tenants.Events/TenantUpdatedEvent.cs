using Cheetah.Core.Tenants.Events;

namespace Cheetah.Tenants.Events;

/// <summary>
/// Event raised when a tenant is updated
/// </summary>
public record TenantUpdatedEvent(
    Guid TenantId,
    string Name,
    bool IsActive,
    DateTime UpdatedAt
) : Core.Tenants.Events.TenantUpdatedEvent(TenantId, Name);
