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

        // Seed initial data (Admin and User roles)
        await SeedInitialDataAsync(dbContext, ct);

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

    /// <summary>
    /// Seeds initial roles and admin user for new tenant database
    /// </summary>
    private async Task SeedInitialDataAsync(IdentityDbContext dbContext, CancellationToken ct)
    {
        // Check if data already seeded
        if (await dbContext.Roles.AnyAsync(ct))
        {
            _logger.LogInformation("Identity database already seeded, skipping seed");
            return;
        }

        _logger.LogInformation("Seeding initial Identity data...");

        // Create Admin role with all permissions
        var adminRole = Domain.Entities.Role.Create("Admin", "System Administrator");
        adminRole.AddPermission("Users.Create");
        adminRole.AddPermission("Users.Edit");
        adminRole.AddPermission("Users.Delete");
        adminRole.AddPermission("Users.View");
        adminRole.AddPermission("Roles.Create");
        adminRole.AddPermission("Roles.Edit");
        adminRole.AddPermission("Roles.Delete");
        adminRole.AddPermission("Roles.View");
        adminRole.AddPermission("Permissions.Assign");
        adminRole.AddPermission("System.Configure");

        dbContext.Roles.Add(adminRole);

        // Create User role with basic permissions
        var userRole = Domain.Entities.Role.Create("User", "Standard User");
        userRole.AddPermission("Users.View");

        dbContext.Roles.Add(userRole);

        // Create default admin user
        var adminUser = Domain.Entities.User.Create(
            email: "admin@cheetah.local",
            passwordHash: _passwordHasher.HashPassword("Admin@123"),
            firstName: "System",
            lastName: "Administrator"
        );
        adminUser.ConfirmEmail();
        adminUser.Activate();

        dbContext.Users.Add(adminUser);

        await dbContext.SaveChangesAsync(ct);

        // Assign Admin role to admin user
        var userRole1 = Domain.Entities.UserRole.Create(adminUser.Id, adminRole.Id);
        dbContext.UserRoles.Add(userRole1);

        await dbContext.SaveChangesAsync(ct);

        _logger.LogInformation("Successfully seeded initial Identity data (2 roles, 1 admin user)");
    }
}
