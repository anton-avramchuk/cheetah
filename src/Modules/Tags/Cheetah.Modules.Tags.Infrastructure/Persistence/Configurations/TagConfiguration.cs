using Cheetah.Modules.Tags.Domain.Entities;
using Cheetah.Modules.Tags.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.Tags.Infrastructure.Persistence.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("Tags", "tags");
        builder.Ignore(e => e.DomainEvents);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(TagsConstants.MaxNameLength).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(TagsConstants.MaxNameLength).IsRequired();
        builder.Property(x => x.Color).HasMaxLength(TagsConstants.MaxColorLength);
        builder.Property(x => x.Group).HasMaxLength(TagsConstants.MaxGroupLength);
        builder.HasIndex(x => x.Name).IsUnique();
        builder.HasIndex(x => x.Group);
    }
}
