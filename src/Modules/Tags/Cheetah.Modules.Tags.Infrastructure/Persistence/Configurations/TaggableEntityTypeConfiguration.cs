using Cheetah.Modules.Tags.Domain.Entities;
using Cheetah.Modules.Tags.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.Tags.Infrastructure.Persistence.Configurations;

public class TaggableEntityTypeConfiguration : IEntityTypeConfiguration<TaggableEntityType>
{
    public void Configure(EntityTypeBuilder<TaggableEntityType> builder)
    {
        builder.ToTable("TaggableEntityTypes", "tags");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(TagsConstants.MaxEntityTypeKeyLength);
        builder.Property(x => x.DisplayName).HasMaxLength(TagsConstants.MaxNameLength).IsRequired();
        builder.Property(x => x.OwnerService).HasMaxLength(TagsConstants.MaxNameLength).IsRequired();
        builder.Property(x => x.AllowedGroups).HasColumnType("text[]");
        builder.HasIndex(x => x.OwnerService);
    }
}
