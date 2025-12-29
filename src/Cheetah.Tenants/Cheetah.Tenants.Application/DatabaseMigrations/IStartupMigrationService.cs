namespace Cheetah.Tenants.Application.DatabaseMigrations;

/// <summary>
/// Service to run migrations for all modules at application startup
/// </summary>
public interface IStartupMigrationService
{
    /// <summary>
    /// Migrates all modules for all tenants
    /// </summary>
    Task MigrateAllModulesAsync(CancellationToken ct = default);
}
