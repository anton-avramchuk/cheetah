using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cheetah.Core.EntityFramework.Migrations;

/// <summary>
/// EF Core implementation of database migrator
/// </summary>
public class EfCoreMigrator<TDbContext> : IDatabaseMigrator
    where TDbContext : DbContext
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EfCoreMigrator<TDbContext>> _logger;

    public EfCoreMigrator(
        IServiceProvider serviceProvider,
        ILogger<EfCoreMigrator<TDbContext>> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        var dbContextName = typeof(TDbContext).Name;

        _logger.LogInformation("Starting migration for {DbContext}...", dbContextName);

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

            // Check if database exists
            var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

            if (!canConnect)
            {
                _logger.LogInformation("Database for {DbContext} does not exist. Creating database and applying all migrations...", dbContextName);

                // This will create the database and apply all migrations
                await dbContext.Database.MigrateAsync(cancellationToken);

                _logger.LogInformation("Successfully created database and applied all migrations for {DbContext}", dbContextName);
                return;
            }

            // Database exists - check for pending migrations
            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
            var pendingMigrationsList = pendingMigrations.ToList();

            if (pendingMigrationsList.Count == 0)
            {
                _logger.LogInformation("{DbContext} is already up to date. No migrations to apply.", dbContextName);
                return;
            }

            _logger.LogInformation(
                "{DbContext} has {Count} pending migration(s): {Migrations}",
                dbContextName,
                pendingMigrationsList.Count,
                string.Join(", ", pendingMigrationsList));

            await dbContext.Database.MigrateAsync(cancellationToken);

            _logger.LogInformation(
                "Successfully applied {Count} migration(s) to {DbContext}",
                pendingMigrationsList.Count,
                dbContextName);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to migrate {DbContext}: {Message}",
                dbContextName,
                ex.Message);
            throw;
        }
    }
}
