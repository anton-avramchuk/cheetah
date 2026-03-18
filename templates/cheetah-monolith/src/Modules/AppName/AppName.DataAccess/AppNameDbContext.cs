using AppName.DataAccess.Configurations;
using AppName.Domain;
using Cheetah.Core.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace AppName.DataAccess;

public class AppNameDbContext(DbContextOptions<AppNameDbContext> options)
    : CrmDbContext<AppNameDbContext>(options)
{
    public DbSet<SampleEntity> SampleEntities => Set<SampleEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new SampleEntityConfiguration());
    }
}
