using Cheetah.Core.Events;

namespace Cheetah.Tenants.Events;

/// <summary>
/// Event raised when a tenant database is successfully created
/// </summary>
public record TenantDatabaseCreatedEvent(
    Guid TenantId,
    string DatabaseName,
    DateTime CreatedAt
) : EventBase;
