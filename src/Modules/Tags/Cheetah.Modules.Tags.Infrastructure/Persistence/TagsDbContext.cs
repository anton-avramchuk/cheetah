using Cheetah.Core.DataAccess.Attributes;
using Cheetah.Core.EntityFramework;
using Cheetah.Modules.Tags.Domain.Entities;
using Cheetah.Modules.Tags.Infrastructure.Persistence.Configurations;
using Cheetah.Modules.Tags.Shared;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Modules.Tags.Infrastructure.Persistence;

[ConnectionStringName(TagsConstants.ConnectionStringName)]
public class TagsDbContext(DbContextOptions<TagsDbContext> options)
    : CrmDbContext<TagsDbContext>(options)
{
    public DbSet<TaggableEntityType> TaggableEntityTypes => Set<TaggableEntityType>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<TagAssignment> TagAssignments => Set<TagAssignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new TaggableEntityTypeConfiguration());
        modelBuilder.ApplyConfiguration(new TagConfiguration());
        modelBuilder.ApplyConfiguration(new TagAssignmentConfiguration());
    }
}
