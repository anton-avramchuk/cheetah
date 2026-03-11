using Cheetah.Core.DataAccess.Attributes;
using Cheetah.Core.EntityFramework;
using Crm.MasterData.DataAccess.Configurations;
using Crm.MasterData.Domain;
using Microsoft.EntityFrameworkCore;

namespace Crm.MasterData.DataAccess;

[ConnectionStringName("MasterData")]
public class MasterDataDbContext(DbContextOptions<MasterDataDbContext> options)
    : CrmDbContext<MasterDataDbContext>(options)
{
    public DbSet<StackItem> StackItems => Set<StackItem>();
    public DbSet<Industry> Industries => Set<Industry>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<WorkFormat> WorkFormats => Set<WorkFormat>();
    public DbSet<CandidateSource> CandidateSources => Set<CandidateSource>();
    public DbSet<SkillCategory> SkillCategories => Set<SkillCategory>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<Location> Locations => Set<Location>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new StackItemConfiguration());
        modelBuilder.ApplyConfiguration(new IndustryConfiguration());
        modelBuilder.ApplyConfiguration(new PositionConfiguration());
        modelBuilder.ApplyConfiguration(new WorkFormatConfiguration());
        modelBuilder.ApplyConfiguration(new CandidateSourceConfiguration());
        modelBuilder.ApplyConfiguration(new SkillCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new SkillConfiguration());
        modelBuilder.ApplyConfiguration(new LocationConfiguration());
    }
}
