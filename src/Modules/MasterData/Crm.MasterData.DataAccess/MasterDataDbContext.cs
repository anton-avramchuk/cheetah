using Cheetah.Core.EntityFramework;
using Crm.MasterData.DataAccess.Configurations;
using Crm.MasterData.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.MasterData.DataAccess;

public class MasterDataDbContext(DbContextOptions<MasterDataDbContext> options)
    : CrmDbContext<MasterDataDbContext>(options)
{
    public DbSet<StackItem> SampleEntities => Set<StackItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new StackItemConfiguration());
    }
}