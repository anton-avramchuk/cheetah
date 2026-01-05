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
public class TenantDatabaseMigrationManager<TTenantCreatedEvent> : IEventHandler<TTenantCreatedEvent>
    where TTenantCreatedEvent : TenantCreatedEvent
{
    private readonly ITenantMigrationService _tenantService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TenantDatabaseMigrationManager<TTenantCreatedEvent>> _logger;

    public TenantDatabaseMigrationManager(
        ITenantMigrationService tenantService,
        IServiceProvider serviceProvider,
        ILogger<TenantDatabaseMigrationManager<TTenantCreatedEvent>> logger)
    {
        _tenantService = tenantService;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Event handler implementation - automatically called when tenant is created
    /// </summary>
    public async ValueTask HandleAsync(TTenantCreatedEvent @event, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Creating and migrating databases for tenant {TenantId} ({TenantName})",
            @event.TenantId, @event.Name);

        try
        {
            await CreateTenantDatabasesAsync(@event.TenantId, ct);

            _logger.LogInformation(
                "Successfully created and migrated all databases for tenant {TenantId}",
                @event.TenantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
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
        var dbContexts = _serviceProvider.GetServices<ITenantBasedDbContext<TTenantCreatedEvent>>();

        foreach (var dbContextPrototype in dbContexts)
        {
            try
            {
                var connectionString = await _tenantService.GetConnectionStringAsync(
                    tenantId,
                    dbContextPrototype.ModuleName,
                    ct);

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    _logger.LogWarning(
                        "No connection string found for tenant {TenantId}, module {ModuleName}. Skipping migration.",
                        tenantId, dbContextPrototype.ModuleName);
                    continue;
                }

                _logger.LogInformation(
                    "Migrating {ModuleName} database for tenant {TenantId}",
                    dbContextPrototype.ModuleName, tenantId);

                await using var tenantDbContext = dbContextPrototype.CreateForTenant(connectionString);
                await tenantDbContext.Database.MigrateAsync(ct);

                _logger.LogInformation(
                    "Successfully migrated {ModuleName} database for tenant {TenantId}",
                    dbContextPrototype.ModuleName, tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
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
        var dbContexts = _serviceProvider.GetServices<ITenantBasedDbContext<TTenantCreatedEvent>>();

        foreach (var dbContextPrototype in dbContexts)
        {
            try
            {
                var connectionString = await _tenantService.GetConnectionStringAsync(
                    tenantId,
                    dbContextPrototype.ModuleName,
                    ct);

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    _logger.LogError(
                        "No connection string found for tenant {TenantId}, module {ModuleName}. Cannot create database.",
                        tenantId, dbContextPrototype.ModuleName);
                    throw new InvalidOperationException(
                        $"No connection string found for tenant {tenantId}, module {dbContextPrototype.ModuleName}");
                }

                _logger.LogInformation(
                    "Creating {ModuleName} database for tenant {TenantId}",
                    dbContextPrototype.ModuleName, tenantId);

                await using var tenantDbContext = dbContextPrototype.CreateForTenant(connectionString);
                await tenantDbContext.Database.MigrateAsync(ct);

                var databaseName = tenantDbContext.Database.GetDbConnection().Database;
                _logger.LogInformation(
                    "Successfully created {ModuleName} database '{DatabaseName}' for tenant {TenantId}",
                    dbContextPrototype.ModuleName, databaseName, tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
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
        _logger.LogInformation("Migrating all tenant-based modules for all active tenants...");

        var tenants = await _tenantService.GetAllActiveTenantsAsync(ct);
        _logger.LogInformation("Found {TenantCount} active tenants to migrate", tenants.Count);

        foreach (var tenant in tenants)
        {
            try
            {
                await MigrateTenantDatabasesAsync(tenant.Id, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to migrate databases for tenant {TenantId} ({TenantName})",
                    tenant.Id, tenant.Name);
                // Continue with other tenants even if one fails
            }
        }

        _logger.LogInformation("All tenants migration completed");
    }
}
