namespace Cheetah.Tenants.Application.DatabaseMigrations;

/// <summary>
/// Manages database migrations for tenant databases
/// </summary>
public interface IDatabaseMigrationManager
{
    /// <summary>
    /// Migrates tenant database to the latest version
    /// </summary>
    Task MigrateTenantDatabaseAsync(Guid tenantId, CancellationToken ct = default);

    /// <summary>
    /// Creates a new database for the tenant and applies all migrations
    /// </summary>
    Task CreateTenantDatabaseAsync(Guid tenantId, CancellationToken ct = default);

    /// <summary>
    /// Migrates all tenant databases (used at application startup)
    /// </summary>
    Task MigrateAllTenantsAsync(CancellationToken ct = default);
}
