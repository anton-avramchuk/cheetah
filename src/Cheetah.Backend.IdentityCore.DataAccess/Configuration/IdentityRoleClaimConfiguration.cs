using Cheetah.Backend.IdentityCore.Domain;
using Cheetah.Backend.IdentityCore.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Backend.IdentityCore.DataAccess.Configuration;

public abstract class IdentityRoleClaimConfiguration : IdentityConfiguration<IdentityRoleClaim>
{
    protected override string TableName => "RoleClaims";

    public override void Configure(EntityTypeBuilder<IdentityRoleClaim> builder)
    {
        builder.ToTable(TableName, Schema);

        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(uc => uc.ClaimType).HasMaxLength(IdentityClaimConstants.MaxClaimTypeLength).IsRequired();
        builder.Property(uc => uc.ClaimValue).HasMaxLength(IdentityClaimConstants.MaxClaimValueLength);

        builder.HasIndex(uc => uc.RoleId);
    }
}