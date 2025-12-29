using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cheetah.Core.EntityFramework.Migrations;

/// <summary>
/// Extension methods for registering database migrations
/// </summary>
public static class DatabaseMigrationServiceCollectionExtensions
{
    /// <summary>
    /// Register database migrator for a specific DbContext
    /// </summary>
    public static IServiceCollection AddDatabaseMigrator<TDbContext>(this IServiceCollection services)
        where TDbContext : DbContext
    {
        // Register the manager (singleton, only once)
        services.TryAddSingleton<DatabaseMigrationManager>();

        // Register the specific migrator
        services.AddTransient<IDatabaseMigrator, EfCoreMigrator<TDbContext>>();

        return services;
    }
}
