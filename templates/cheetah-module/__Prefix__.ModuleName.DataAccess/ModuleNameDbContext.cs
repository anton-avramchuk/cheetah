using Cheetah.Core.EntityFramework;
using __Prefix__.ModuleName.DataAccess.Configurations;
using __Prefix__.ModuleName.Domain;
using Microsoft.EntityFrameworkCore;

namespace __Prefix__.ModuleName.DataAccess;

public class ModuleNameDbContext(DbContextOptions<ModuleNameDbContext> options)
    : CrmDbContext<ModuleNameDbContext>(options)
{
    public DbSet<SampleEntity> SampleEntities => Set<SampleEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new SampleEntityConfiguration());
    }
}
