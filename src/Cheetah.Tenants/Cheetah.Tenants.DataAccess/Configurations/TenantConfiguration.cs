using Cheetah.Core.EntityFramework.Tenants.Configurations;
using Cheetah.Tenants.Domain.Entities;
using Cheetah.Tenants.Events;
using Cheetah.Tenants.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Tenants.DataAccess.Configurations;

public class TenantConfiguration : TenantEntityConfiguration<Tenant, TenantCreatedEvent, TenantUpdatedEvent, TenantDeactivatedEvent, TenantActivatedEvent>
{
    protected override TenantEntityConfigurationOptions Options { get; } = new()
    {
        TableName = "Tenants",
        NameMaxLength = TenantConstants.MaxNameLength,
        DescriptionMaxLength = TenantConstants.MaxDescriptionLength,
        NameIndexName = "IX_Tenants_Name",
        IsActiveIndexName = "IX_Tenants_IsActive",
        CreatedAtIndexName = "IX_Tenants_CreatedAt",
        CreateNameUniqueIndex = true,
        CreateIsActiveIndex = true,
        CreateCreatedAtIndex = true
    };

    protected override void ConfigureAdditionalProperties(EntityTypeBuilder<Tenant> builder)
    {
        // NormalizedName - required, unique
        builder.Property(t => t.NormalizedName)
            .IsRequired()
            .HasMaxLength(TenantConstants.MaxNormalizedNameLength);

        builder.HasIndex(t => t.NormalizedName)
            .IsUnique()
            .HasDatabaseName("IX_Tenants_NormalizedName");

        // Subdomain - optional, unique when not null
        builder.Property(t => t.Subdomain)
            .HasMaxLength(TenantConstants.MaxSubdomainLength);

        builder.HasIndex(t => t.Subdomain)
            .IsUnique()
            .HasDatabaseName("IX_Tenants_Subdomain");

        // ConnectionStrings relationship
        builder.HasMany(t => t.ConnectionStrings)
            .WithOne()
            .HasForeignKey(cs => cs.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
