using Cheetah.Core.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cheetah.Tenants.Application.DatabaseMigrations;

/// <summary>
/// Coordinates database migrations for all modules at application startup
/// </summary>
[Export(LifetimeType.Singleton, typeof(IStartupMigrationService))]
public class StartupMigrationService : IStartupMigrationService
{
    private readonly IEnumerable<IDatabaseMigrationManager> _migrationManagers;
    private readonly ILogger<StartupMigrationService> _logger;

    public StartupMigrationService(
        IEnumerable<IDatabaseMigrationManager> migrationManagers,
        ILogger<StartupMigrationService> logger)
    {
        _migrationManagers = migrationManagers;
        _logger = logger;
    }

    public async Task MigrateAllModulesAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Starting database migrations for all modules...");

        foreach (var manager in _migrationManagers)
        {
            try
            {
                _logger.LogInformation("Migrating module: {ManagerType}", manager.GetType().Name);
                await manager.MigrateAllTenantsAsync(ct);
                _logger.LogInformation("Successfully migrated module: {ManagerType}", manager.GetType().Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to migrate module: {ManagerType}", manager.GetType().Name);
                throw;
            }
        }

        _logger.LogInformation("All database migrations completed successfully");
    }
}
