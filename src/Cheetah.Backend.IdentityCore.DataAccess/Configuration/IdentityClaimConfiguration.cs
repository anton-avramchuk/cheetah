using Cheetah.Backend.IdentityCore.Domain;
using Cheetah.Backend.IdentityCore.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Backend.IdentityCore.DataAccess.Configuration;

public abstract class IdentityClaimConfiguration : IdentityConfiguration<IdentityUserClaim>
{
    public override void Configure(EntityTypeBuilder<IdentityUserClaim> builder)
    {
        builder.ToTable(TableName, Schema);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(uc => uc.ClaimType).HasMaxLength(IdentityClaimConstants.MaxClaimTypeLength).IsRequired();
        builder.Property(uc => uc.ClaimValue).HasMaxLength(IdentityClaimConstants.MaxClaimValueLength);

        builder.HasIndex(uc => uc.UserId);
    }
}