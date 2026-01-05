using Cheetah.Core.DataAccess.Attributes;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.EntityFramework.Tenants;
using Cheetah.Tenants.Domain.Entities;
using Cheetah.Tenants.Events;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Tenants.DataAccess;

[ConnectionStringName("Tenants")]
[Export(LifetimeType.Scoped)]
public class TenantsDbContext(DbContextOptions<TenantsDbContext> options)
    : CrmTenantsDbContext<TenantsDbContext, Tenant, TenantCreatedEvent, TenantUpdatedEvent, TenantDeactivatedEvent,
        TenantActivatedEvent>(options)
{
    public DbSet<TenantConnectionString> TenantConnectionStrings => Set<TenantConnectionString>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TenantsDbContext).Assembly);
    }
}
