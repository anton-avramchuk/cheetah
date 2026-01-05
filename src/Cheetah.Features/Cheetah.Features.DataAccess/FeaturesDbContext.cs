using Cheetah.Core.DataAccess.Attributes;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.EntityFramework;
using Cheetah.Features.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Features.DataAccess;

[ConnectionStringName("Features")]
[Export(LifetimeType.Scoped)]
public class FeaturesDbContext(DbContextOptions<FeaturesDbContext> options)
    : CrmDbContext<FeaturesDbContext>(options)
{
    public DbSet<Feature> Features => Set<Feature>();
    public DbSet<TenantFeature> TenantFeatures => Set<TenantFeature>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FeaturesDbContext).Assembly);
    }
}
