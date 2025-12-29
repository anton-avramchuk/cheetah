using Cheetah.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cheetah.Core.EntityFramework.Migrations;


/// <summary>
/// Manages migration execution for all registered database migrators
/// </summary>
[Export(LifetimeType.Singleton)]
public class DatabaseMigrationManager(
    IServiceProvider serviceProvider,
    ILogger<DatabaseMigrationManager> logger)
{
    /// <summary>
    /// Apply all pending migrations for all registered databases
    /// </summary>
    public async Task MigrateAllAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting database migration process...");

        var migrators = serviceProvider.GetServices<IDatabaseMigrator>().ToList();

        if (migrators.Count == 0)
        {
            logger.LogInformation("No database migrators registered. Skipping migration.");
            return;
        }

        logger.LogInformation("Found {Count} database migrator(s) to execute", migrators.Count);

        var failedMigrations = new List<Exception>();

        foreach (var migrator in migrators)
        {
            try
            {
                await migrator.MigrateAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Migration failed for {MigratorType}", migrator.GetType().Name);
                failedMigrations.Add(ex);
            }
        }

        if (failedMigrations.Count > 0)
        {
            logger.LogError(
                "Database migration completed with {Count} error(s)",
                failedMigrations.Count);

            throw new AggregateException(
                "One or more database migrations failed. See inner exceptions for details.",
                failedMigrations);
        }

        logger.LogInformation("All database migrations completed successfully");
    }
}
