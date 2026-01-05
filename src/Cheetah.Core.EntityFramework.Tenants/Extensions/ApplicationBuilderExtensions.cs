using Cheetah.Core.EntityFramework.Tenants.Migrations;
using Cheetah.Core.Tenants.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cheetah.Core.EntityFramework.Tenants.Extensions;

public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Migrates all tenant-based databases for all active tenants on application startup
    /// Call this after app.Build() and before app.Run()
    /// </summary>
    public static IApplicationBuilder MigrateTenantDatabases<TTenantCreatedEvent>(
        this IApplicationBuilder app)
        where TTenantCreatedEvent : TenantCreatedEvent
    {
        using var scope = app.ApplicationServices.CreateScope();
        var migrationManager = scope.ServiceProvider.GetRequiredService<TenantDatabaseMigrationManager<TTenantCreatedEvent>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TenantDatabaseMigrationManager<TTenantCreatedEvent>>>();

        try
        {
            logger.LogInformation("Starting tenant database migrations on application startup...");
            migrationManager.MigrateAllTenantsAsync().GetAwaiter().GetResult();
            logger.LogInformation("Tenant database migrations completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to migrate tenant databases on application startup");
            throw;
        }

        return app;
    }
}
