using Cheetah.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Identity.DataAccess.Configurations;

public class RoleClaimConfiguration : IEntityTypeConfiguration<RoleClaim>
{
    public void Configure(EntityTypeBuilder<RoleClaim> builder)
    {
        builder.ToTable("RoleClaims");

        builder.HasKey(rc => rc.Id);

        builder.Property(rc => rc.RoleId)
            .IsRequired();

        builder.Property(rc => rc.ClaimType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(rc => rc.ClaimValue)
            .IsRequired()
            .HasMaxLength(200);

        // Unique constraint: one role can't have duplicate claims
        builder.HasIndex(rc => new { rc.RoleId, rc.ClaimType, rc.ClaimValue })
            .IsUnique();
    }
}
