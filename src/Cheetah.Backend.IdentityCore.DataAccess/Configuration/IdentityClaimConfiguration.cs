using Cheetah.Backend.IdentityCore.Domain;
using Cheetah.Backend.IdentityCore.Shared.Constants;
using Cheetah.Core.EntityFramework.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Backend.IdentityCore.DataAccess.Configuration;

public abstract class IdentityClaimConfiguration : EntityConfiguration<IdentityUserClaim, Guid, IdentityClaimConfigurationOptions>
{
    protected override IdentityClaimConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<IdentityUserClaim> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(uc => uc.ClaimType).HasMaxLength(Options.ClaimTypeMaxLength).IsRequired();
        builder.Property(uc => uc.ClaimValue).HasMaxLength(Options.ClaimValueMaxLength).IsRequired();

        builder.HasIndex(uc => uc.UserId);
    }
}

public class IdentityClaimConfigurationOptions : EntityConfigurationOptions<IdentityUserClaim, Guid>
{
    public override string Schema => Constants.DefaultSchema;

    public int ClaimTypeMaxLength { get; set; } = IdentityClaimConstants.MaxClaimTypeLength;
    
    public int ClaimValueMaxLength { get; set; }= IdentityClaimConstants.MaxClaimValueLength;
}