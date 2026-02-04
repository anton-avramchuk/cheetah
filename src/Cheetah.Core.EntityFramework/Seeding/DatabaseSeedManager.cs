using Cheetah.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cheetah.Core.EntityFramework.Seeding;

/// <summary>
/// Manages seeding execution for all registered database seeders
/// </summary>
[Export(LifetimeType.Singleton)]
public class DatabaseSeedManager(
    IServiceProvider serviceProvider,
    ILogger<DatabaseSeedManager> logger)
{
    /// <summary>
    /// Execute all registered seeders in order
    /// </summary>
    public async Task SeedAllAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting database seeding process...");

        using var scope = serviceProvider.CreateScope();

        var seeders = scope.ServiceProvider
            .GetServices<IDatabaseSeeder>()
            .OrderBy(s => s.Order)
            .ToList();

        if (seeders.Count == 0)
        {
            logger.LogInformation("No database seeders registered. Skipping seeding.");
            return;
        }

        logger.LogInformation("Found {Count} database seeder(s) to execute", seeders.Count);

        var failedSeeders = new List<(string Name, Exception Exception)>();

        foreach (var seeder in seeders)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var seederName = seeder.GetType().Name;

            try
            {
                logger.LogDebug("Executing seeder: {SeederName}", seederName);
                await seeder.SeedAsync(cancellationToken);
                logger.LogDebug("Seeder completed: {SeederName}", seederName);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Seeding failed for {SeederName}", seederName);
                failedSeeders.Add((seederName, ex));
            }
        }

        if (failedSeeders.Count > 0)
        {
            logger.LogError(
                "Database seeding completed with {Count} error(s): {FailedSeeders}",
                failedSeeders.Count,
                string.Join(", ", failedSeeders.Select(f => f.Name)));

            throw new AggregateException(
                "One or more database seeders failed. See inner exceptions for details.",
                failedSeeders.Select(f => f.Exception));
        }

        logger.LogInformation("All database seeders completed successfully");
    }
}
