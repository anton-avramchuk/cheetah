using Cheetah.Tenants.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Tenants.DataAccess.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(t => t.NormalizedName)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(t => t.NormalizedName)
            .IsUnique();

        builder.Property(t => t.Subdomain)
            .HasMaxLength(128);

        builder.HasIndex(t => t.Subdomain)
            .IsUnique();

        builder.Property(t => t.IsActive)
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.UpdatedAt);

        builder.HasMany(t => t.ConnectionStrings)
            .WithOne()
            .HasForeignKey(cs => cs.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(t => t.DomainEvents);
    }
}
