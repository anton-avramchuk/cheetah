using Cheetah.Core.Tenants.Events;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Cheetah.Core.EntityFramework.Tenants;

/// <summary>
/// Interface for DbContext that supports tenant-based database connections
/// </summary>
/// <typeparam name="TTenantCreatedEvent">Type of tenant created event</typeparam>
public interface ITenantBasedDbContext<TTenantCreatedEvent> : IDisposable, IAsyncDisposable
    where TTenantCreatedEvent : TenantCreatedEvent
{
    /// <summary>
    /// Database facade for database operations
    /// </summary>
    DatabaseFacade Database { get; }

    /// <summary>
    /// Module name for logging and identification
    /// </summary>
    string ModuleName { get; }

    /// <summary>
    /// Creates a new instance of the DbContext configured for a specific tenant
    /// </summary>
    /// <param name="connectionString">Tenant-specific connection string</param>
    /// <returns>New DbContext instance</returns>
    ITenantBasedDbContext<TTenantCreatedEvent> CreateForTenant(string connectionString);
}
