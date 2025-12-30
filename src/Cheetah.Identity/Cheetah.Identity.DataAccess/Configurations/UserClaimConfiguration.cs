using Cheetah.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Identity.DataAccess.Configurations;

public class UserClaimConfiguration : IEntityTypeConfiguration<UserClaim>
{
    public void Configure(EntityTypeBuilder<UserClaim> builder)
    {
        builder.ToTable("UserClaims");

        builder.HasKey(uc => uc.Id);

        builder.Property(uc => uc.UserId)
            .IsRequired();

        builder.Property(uc => uc.ClaimType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(uc => uc.ClaimValue)
            .IsRequired()
            .HasMaxLength(200);

        // Unique constraint: one user can't have duplicate claims
        builder.HasIndex(uc => new { uc.UserId, uc.ClaimType, uc.ClaimValue })
            .IsUnique();
    }
}
