using Cheetah.Core.Events;
using Cheetah.Core.Tenants.Events;
using Cheetah.Core.Tenants.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cheetah.Core.EntityFramework.Tenants.Migrations;

/// <summary>
/// Manages database migrations across multiple tenants for all registered tenant-based modules
/// Automatically subscribes to TenantCreatedEvent and creates/migrates databases
/// </summary>
/// <typeparam name="TTenantCreatedEvent">Type of tenant created event</typeparam>
public class TenantDatabaseMigrationManager<TTenantCreatedEvent>(
    IServiceProvider serviceProvider,
    ILogger<TenantDatabaseMigrationManager<TTenantCreatedEvent>> logger)
    : IEventHandler<TTenantCreatedEvent>
    where TTenantCreatedEvent : TenantCreatedEvent
{
    /// <summary>
    /// Event handler implementation - automatically called when tenant is created
    /// </summary>
    public async ValueTask HandleAsync(TTenantCreatedEvent @event, CancellationToken ct = default)
    {
        logger.LogInformation(
            "Creating and migrating databases for tenant {TenantId} ({TenantName})",
            @event.TenantId, @event.Name);

        try
        {
            await CreateTenantDatabasesAsync(@event.TenantId, ct);

            logger.LogInformation(
                "Successfully created and migrated all databases for tenant {TenantId}",
                @event.TenantId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to create/migrate databases for tenant {TenantId}",
                @event.TenantId);
            throw;
        }
    }

    /// <summary>
    /// Migrates all tenant-based DbContexts for a specific tenant
    /// </summary>
    public async Task MigrateTenantDatabasesAsync(Guid tenantId, CancellationToken ct = default)
    {
        using var scope = serviceProvider.CreateScope();
        var tenantService = scope.ServiceProvider.GetRequiredService<ITenantMigrationService>();
        var dbContexts = serviceProvider.GetServices<ITenantBasedDbContext<TTenantCreatedEvent>>();

        foreach (var dbContextPrototype in dbContexts)
        {
            try
            {
                var connectionString = await tenantService.GetConnectionStringAsync(
                    tenantId,
                    dbContextPrototype.ModuleName,
                    ct);

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    logger.LogWarning(
                        "No connection string found for tenant {TenantId}, module {ModuleName}. Skipping migration.",
                        tenantId, dbContextPrototype.ModuleName);
                    continue;
                }

                logger.LogInformation(
                    "Migrating {ModuleName} database for tenant {TenantId}",
                    dbContextPrototype.ModuleName, tenantId);

                await using var tenantDbContext = dbContextPrototype.CreateForTenant(connectionString);
                await tenantDbContext.Database.MigrateAsync(ct);

                logger.LogInformation(
                    "Successfully migrated {ModuleName} database for tenant {TenantId}",
                    dbContextPrototype.ModuleName, tenantId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Failed to migrate {ModuleName} database for tenant {TenantId}",
                    dbContextPrototype.ModuleName, tenantId);
                throw;
            }
        }
    }

    /// <summary>
    /// Creates databases for all tenant-based DbContexts for a specific tenant
    /// </summary>
    public async Task CreateTenantDatabasesAsync(Guid tenantId, CancellationToken ct = default)
    {
        using var scope = serviceProvider.CreateScope();
        var tenantService = scope.ServiceProvider.GetRequiredService<ITenantMigrationService>();
        var dbContexts = serviceProvider.GetServices<ITenantBasedDbContext<TTenantCreatedEvent>>();

        foreach (var dbContextPrototype in dbContexts)
        {
            try
            {
                var connectionString = await tenantService.GetConnectionStringAsync(
                    tenantId,
                    dbContextPrototype.ModuleName,
                    ct);

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    logger.LogError(
                        "No connection string found for tenant {TenantId}, module {ModuleName}. Cannot create database.",
                        tenantId, dbContextPrototype.ModuleName);
                    throw new InvalidOperationException(
                        $"No connection string found for tenant {tenantId}, module {dbContextPrototype.ModuleName}");
                }

                logger.LogInformation(
                    "Creating {ModuleName} database for tenant {TenantId}",
                    dbContextPrototype.ModuleName, tenantId);

                await using var tenantDbContext = dbContextPrototype.CreateForTenant(connectionString);
                await tenantDbContext.Database.MigrateAsync(ct);

                var databaseName = tenantDbContext.Database.GetDbConnection().Database;
                logger.LogInformation(
                    "Successfully created {ModuleName} database '{DatabaseName}' for tenant {TenantId}",
                    dbContextPrototype.ModuleName, databaseName, tenantId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Failed to create {ModuleName} database for tenant {TenantId}",
                    dbContextPrototype.ModuleName, tenantId);
                throw;
            }
        }
    }

    /// <summary>
    /// Migrates all tenant-based DbContexts for all active tenants
    /// </summary>
    public async Task MigrateAllTenantsAsync(CancellationToken ct = default)
    {
        logger.LogInformation("Migrating all tenant-based modules for all active tenants...");

        using var scope = serviceProvider.CreateScope();
        var tenantService = scope.ServiceProvider.GetRequiredService<ITenantMigrationService>();

        var tenants = await tenantService.GetAllActiveTenantsAsync(ct);
        logger.LogInformation("Found {TenantCount} active tenants to migrate", tenants.Count);

        foreach (var tenant in tenants)
        {
            try
            {
                await MigrateTenantDatabasesAsync(tenant.Id, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Failed to migrate databases for tenant {TenantId} ({TenantName})",
                    tenant.Id, tenant.Name);
                // Continue with other tenants even if one fails
            }
        }

        logger.LogInformation("All tenants migration completed");
    }
}
