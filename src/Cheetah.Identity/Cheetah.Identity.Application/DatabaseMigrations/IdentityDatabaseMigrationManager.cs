using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Cheetah.Identity.Application.Services;
using Cheetah.Identity.DataAccess;
using Cheetah.Tenants.Client.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Cheetah.Identity.Application.DatabaseMigrations;

/// <summary>
/// Manages database migrations for Identity module across tenants
/// </summary>
[Export(LifetimeType.Scoped, typeof(IDatabaseMigrationManager))]
public class IdentityDatabaseMigrationManager : IDatabaseMigrationManager
{
    private readonly ITenantClientService _tenantClient;
    private readonly ILogger<IdentityDatabaseMigrationManager> _logger;
    private readonly IEventBus _eventBus;
    private readonly IConfiguration _configuration;
    private readonly IPasswordHasher _passwordHasher;

    public IdentityDatabaseMigrationManager(
        ITenantClientService tenantClient,
        ILogger<IdentityDatabaseMigrationManager> logger,
        IEventBus eventBus,
        IConfiguration configuration,
        IPasswordHasher passwordHasher)
    {
        _tenantClient = tenantClient;
        _logger = logger;
        _eventBus = eventBus;
        _configuration = configuration;
        _passwordHasher = passwordHasher;
    }

    public async Task MigrateTenantDatabaseAsync(Guid tenantId, CancellationToken ct = default)
    {
        var connectionString = await _tenantClient.GetConnectionStringAsync(tenantId, "Identity", ct);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            _logger.LogWarning("No Identity connection string found for tenant {TenantId}", tenantId);
            return;
        }

        _logger.LogInformation("Migrating Identity database for tenant {TenantId}", tenantId);

        var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        await using var dbContext = new IdentityDbContext(optionsBuilder.Options);
        await dbContext.Database.MigrateAsync(ct);

        _logger.LogInformation("Successfully migrated Identity database for tenant {TenantId}", tenantId);
    }

    public async Task CreateTenantDatabaseAsync(Guid tenantId, CancellationToken ct = default)
    {
        var connectionString = await _tenantClient.GetConnectionStringAsync(tenantId, "Identity", ct);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            _logger.LogError("No Identity connection string found for tenant {TenantId}. Cannot create database.", tenantId);
            throw new InvalidOperationException($"No Identity connection string found for tenant {tenantId}");
        }

        _logger.LogInformation("Creating Identity database for tenant {TenantId}", tenantId);

        var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        await using var dbContext = new IdentityDbContext(optionsBuilder.Options);

        // Create database if it doesn't exist and apply migrations
        await dbContext.Database.MigrateAsync(ct);
        
        var databaseName = dbContext.Database.GetDbConnection().Database;
        _logger.LogInformation("Successfully created Identity database '{DatabaseName}' for tenant {TenantId}",
            databaseName, tenantId);
    }

    public async Task MigrateAllTenantsAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Migrating Identity module for all active tenants...");

        var tenants = await _tenantClient.GetAllActiveAsync(ct);

        _logger.LogInformation("Found {TenantCount} active tenants to migrate", tenants.Count);

        foreach (var tenant in tenants)
        {
            try
            {
                await MigrateTenantDatabaseAsync(tenant.Id, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to migrate Identity database for tenant {TenantId} ({TenantName})",
                    tenant.Id, tenant.Name);
                // Continue with other tenants even if one fails
            }
        }

        _logger.LogInformation("Identity module migration completed");
    }
}
