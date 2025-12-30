namespace Cheetah.Identity.Application.Services;

/// <summary>
/// Manages database migrations for Identity module across tenants
/// </summary>
public interface IDatabaseMigrationManager
{
    /// <summary>
    /// Migrates Identity database for a specific tenant
    /// </summary>
    Task MigrateTenantDatabaseAsync(Guid tenantId, CancellationToken ct = default);

    /// <summary>
    /// Creates and migrates Identity database for a newly created tenant
    /// </summary>
    Task CreateTenantDatabaseAsync(Guid tenantId, CancellationToken ct = default);

    /// <summary>
    /// Migrates Identity databases for all active tenants
    /// </summary>
    Task MigrateAllTenantsAsync(CancellationToken ct = default);
}
