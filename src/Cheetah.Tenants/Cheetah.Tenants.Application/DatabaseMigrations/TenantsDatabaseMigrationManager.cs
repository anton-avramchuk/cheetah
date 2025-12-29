using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Cheetah.Tenants.Application.Services;
using Cheetah.Tenants.DataAccess;
using Cheetah.Tenants.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Cheetah.Tenants.Application.DatabaseMigrations;

/// <summary>
/// Manages database migrations for tenant databases
/// </summary>
[Export(LifetimeType.Scoped, typeof(IDatabaseMigrationManager))]
public class TenantsDatabaseMigrationManager : IDatabaseMigrationManager
{
    private readonly ITenantStore _tenantStore;
    private readonly ILogger<TenantsDatabaseMigrationManager> _logger;
    private readonly IEventBus _eventBus;
    private readonly IConfiguration _configuration;

    public TenantsDatabaseMigrationManager(
        ITenantStore tenantStore,
        ILogger<TenantsDatabaseMigrationManager> logger,
        IEventBus eventBus,
        IConfiguration configuration)
    {
        _tenantStore = tenantStore;
        _logger = logger;
        _eventBus = eventBus;
        _configuration = configuration;
    }

    public async Task MigrateTenantDatabaseAsync(Guid tenantId, CancellationToken ct = default)
    {
        var connectionString = await _tenantStore.GetConnectionStringAsync(tenantId, "Default", ct);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            _logger.LogWarning("No connection string found for tenant {TenantId}", tenantId);
            return;
        }

        _logger.LogInformation("Migrating Tenants database for tenant {TenantId}", tenantId);

        var optionsBuilder = new DbContextOptionsBuilder<TenantsDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        await using var dbContext = new TenantsDbContext(optionsBuilder.Options);
        await dbContext.Database.MigrateAsync(ct);

        _logger.LogInformation("Successfully migrated Tenants database for tenant {TenantId}", tenantId);
    }

    public async Task CreateTenantDatabaseAsync(Guid tenantId, CancellationToken ct = default)
    {
        var connectionString = await _tenantStore.GetConnectionStringAsync(tenantId, "Default", ct);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            _logger.LogError("No connection string found for tenant {TenantId}. Cannot create database.", tenantId);
            throw new InvalidOperationException($"No connection string found for tenant {tenantId}");
        }

        _logger.LogInformation("Creating Tenants database for tenant {TenantId}", tenantId);

        var optionsBuilder = new DbContextOptionsBuilder<TenantsDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        await using var dbContext = new TenantsDbContext(optionsBuilder.Options);

        // Create database if it doesn't exist and apply migrations
        await dbContext.Database.MigrateAsync(ct);

        // Publish event that database was created
        var databaseName = dbContext.Database.GetDbConnection().Database;
        await _eventBus.PublishAsync(new TenantDatabaseCreatedEvent(
            tenantId,
            databaseName,
            DateTime.UtcNow), ct);

        _logger.LogInformation("Successfully created Tenants database '{DatabaseName}' for tenant {TenantId}",
            databaseName, tenantId);
    }

    public async Task MigrateAllTenantsAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Migrating Tenants module for all active tenants...");

        var tenants = await _tenantStore.GetAllActiveTenantsAsync(ct);

        _logger.LogInformation("Found {TenantCount} active tenants to migrate", tenants.Count);

        foreach (var tenant in tenants)
        {
            try
            {
                await MigrateTenantDatabaseAsync(tenant.Id, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to migrate Tenants database for tenant {TenantId} ({TenantName})",
                    tenant.Id, tenant.Name);
                // Continue with other tenants even if one fails
            }
        }

        _logger.LogInformation("Tenants module migration completed");
    }
}
