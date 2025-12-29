using Cheetah.Core.DataAccess.Attributes;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.EntityFramework;
using Cheetah.Tenants.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Tenants.DataAccess;

[ConnectionStringName("Tenants")]
[Export(LifetimeType.Scoped)]
public class TenantsDbContext : CrmDbContext<TenantsDbContext>
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
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
