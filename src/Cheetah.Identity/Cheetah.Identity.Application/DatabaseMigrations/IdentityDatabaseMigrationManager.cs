using Cheetah.Core.DependencyInjection;
using Cheetah.Core.EntityFramework.Tenants.Migrations;
using Cheetah.Core.Tenants.Events;
using Cheetah.Core.Tenants.Services;
using Cheetah.Identity.Application.Services;
using Microsoft.Extensions.Logging;

namespace Cheetah.Identity.Application.DatabaseMigrations;

/// <summary>
/// Manages database migrations for Identity module across tenants
/// </summary>
[Export(LifetimeType.Scoped, typeof(IDatabaseMigrationManager))]
public class IdentityDatabaseMigrationManager : TenantDatabaseMigrationManager<TenantCreatedEvent>, IDatabaseMigrationManager
{
    public IdentityDatabaseMigrationManager(
        ITenantMigrationService tenantService,
        IServiceProvider serviceProvider,
        ILogger<TenantDatabaseMigrationManager<TenantCreatedEvent>> logger)
        : base(tenantService, serviceProvider, logger)
    {
    }

    public async Task MigrateTenantDatabaseAsync(Guid tenantId, CancellationToken ct = default)
    {
        await MigrateTenantDatabasesAsync(tenantId, ct);
    }

    public async Task CreateTenantDatabaseAsync(Guid tenantId, CancellationToken ct = default)
    {
        await CreateTenantDatabasesAsync(tenantId, ct);
    }
}
