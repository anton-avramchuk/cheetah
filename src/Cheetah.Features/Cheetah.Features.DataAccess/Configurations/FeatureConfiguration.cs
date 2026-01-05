using Cheetah.Features.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Features.DataAccess.Configurations;

public class FeatureConfiguration : IEntityTypeConfiguration<Feature>
{
    public void Configure(EntityTypeBuilder<Feature> builder)
    {
        builder.ToTable("Features");

        // Primary key - string ID
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id)
            .IsRequired()
            .HasMaxLength(128);

        // Properties
        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(f => f.DisplayName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(f => f.Description)
            .HasMaxLength(1000);

        builder.Property(f => f.Group)
            .HasMaxLength(128);

        builder.Property(f => f.IsEnabledByDefault)
            .IsRequired();

        builder.Property(f => f.CreatedAt)
            .IsRequired();

        builder.Property(f => f.UpdatedAt);

        // Ignore domain events collection
        builder.Ignore(f => f.DomainEvents);

        // Indexes
        builder.HasIndex(f => f.Group)
            .HasDatabaseName("IX_Features_Group");

        builder.HasIndex(f => f.IsEnabledByDefault)
            .HasDatabaseName("IX_Features_IsEnabledByDefault");
    }
}
