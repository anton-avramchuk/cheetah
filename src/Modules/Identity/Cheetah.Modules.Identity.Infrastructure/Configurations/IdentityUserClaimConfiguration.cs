using Cheetah.Core.EntityFramework.Configuration;
using Cheetah.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.Identity.Infrastructure.Configurations;

public abstract class IdentityUserClaimConfigurationOptions
    : EntityConfigurationOptions<IdentityUserClaim, Guid>
{
    public override string TableName => "UserClaims";
    public override string Schema => "identity";
}

public abstract class IdentityUserClaimConfiguration<TConfiguration>
    : EntityConfiguration<IdentityUserClaim, Guid, TConfiguration>
    where TConfiguration : IdentityUserClaimConfigurationOptions
{
    public override void Configure(EntityTypeBuilder<IdentityUserClaim> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.ClaimType)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.ClaimValue)
            .IsRequired()
            .HasMaxLength(1024);

        builder.HasIndex(x => x.UserId);
    }
}
