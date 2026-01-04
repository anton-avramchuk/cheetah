using Cheetah.Core.DataAccess.Attributes;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.EntityFramework.Tenants;
using Cheetah.Tenants.Domain.Entities;
using Cheetah.Tenants.Events;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Tenants.DataAccess;

[ConnectionStringName("Tenants")]
[Export(LifetimeType.Scoped)]
public class TenantsDbContext : CrmTenantsDbContext<TenantsDbContext, Tenant, TenantCreatedEvent, TenantUpdatedEvent, TenantDeactivatedEvent, TenantActivatedEvent>
{
    public DbSet<TenantConnectionString> TenantConnectionStrings => Set<TenantConnectionString>();

    public TenantsDbContext(DbContextOptions<TenantsDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TenantsDbContext).Assembly);
    }
}
