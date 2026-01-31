using Cheetah.Core.EntityFramework;
using Crm.Features.DataAccess.Configurations;
using Crm.Features.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.Features.DataAccess;

public class CrmFeatureDbContext(DbContextOptions<CrmFeatureDbContext> options)
    : CrmDbContext<CrmFeatureDbContext>(options)
{
    public DbSet<Feature> Features { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new FeatureConfiguration());
    }
}