using Cheetah.Modules.Tags.Domain.Entities;
using Cheetah.Modules.Tags.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.Tags.Infrastructure.Persistence.Configurations;

public class TagAssignmentConfiguration : IEntityTypeConfiguration<TagAssignment>
{
    public void Configure(EntityTypeBuilder<TagAssignment> builder)
    {
        builder.ToTable("TagAssignments", "tags");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EntityType).HasMaxLength(TagsConstants.MaxEntityTypeKeyLength).IsRequired();

        // запрет дублей: один тэг на сущность не более одного раза
        builder.HasIndex(x => new { x.EntityType, x.EntityId, x.TagId }).IsUnique();
        // обратный поиск «сущности по тэгу»
        builder.HasIndex(x => new { x.EntityType, x.TagId });
    }
}
