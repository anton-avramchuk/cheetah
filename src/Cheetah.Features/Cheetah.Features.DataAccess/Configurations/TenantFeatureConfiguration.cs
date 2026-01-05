using Cheetah.Features.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Features.DataAccess.Configurations;

public class TenantFeatureConfiguration : IEntityTypeConfiguration<TenantFeature>
{
    public void Configure(EntityTypeBuilder<TenantFeature> builder)
    {
        builder.ToTable("TenantFeatures");

        // Primary key
        builder.HasKey(tf => tf.Id);

        // Properties
        builder.Property(tf => tf.TenantId)
            .IsRequired();

        builder.Property(tf => tf.FeatureId)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(tf => tf.IsEnabled)
            .IsRequired();

        builder.Property(tf => tf.EnabledAt);

        builder.Property(tf => tf.DisabledAt);

        builder.Property(tf => tf.CreatedAt)
            .IsRequired();

        builder.Property(tf => tf.UpdatedAt)
            .IsRequired();

        // Unique constraint: one feature per tenant
        builder.HasIndex(tf => new { tf.TenantId, tf.FeatureId })
            .IsUnique()
            .HasDatabaseName("IX_TenantFeatures_TenantId_FeatureId");

        // Index for querying by tenant
        builder.HasIndex(tf => tf.TenantId)
            .HasDatabaseName("IX_TenantFeatures_TenantId");

        // Index for querying by feature
        builder.HasIndex(tf => tf.FeatureId)
            .HasDatabaseName("IX_TenantFeatures_FeatureId");

        // Index for enabled features
        builder.HasIndex(tf => new { tf.TenantId, tf.IsEnabled })
            .HasDatabaseName("IX_TenantFeatures_TenantId_IsEnabled");
    }
}
