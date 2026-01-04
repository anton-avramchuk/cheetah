using Cheetah.Core.Tenants.Events;

namespace Cheetah.Tenants.Events;

/// <summary>
/// Event raised when a new tenant is created
/// </summary>
public record TenantCreatedEvent(
    Guid TenantId,
    string Name,
    string? Subdomain,
    DateTime CreatedAt
) : Core.Tenants.Events.TenantCreatedEvent(TenantId, Name);
