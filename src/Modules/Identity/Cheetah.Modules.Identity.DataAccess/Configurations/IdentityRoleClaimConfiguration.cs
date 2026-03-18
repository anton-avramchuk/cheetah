using Cheetah.Core.EntityFramework.Configuration;
using Cheetah.Core.Identity.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.Identity.DataAccess.Configurations;

public abstract class IdentityRoleClaimConfigurationOptions
    : EntityConfigurationOptions<IdentityRoleClaim, Guid>
{
    public override string TableName => "RoleClaims";
    public override string Schema => "identity";
}

public abstract class IdentityRoleClaimConfiguration<TConfiguration>
    : EntityConfiguration<IdentityRoleClaim, Guid, TConfiguration>
    where TConfiguration : IdentityRoleClaimConfigurationOptions
{
    public override void Configure(EntityTypeBuilder<IdentityRoleClaim> builder)
    {
        base.Configure(builder);

        builder.Property(x => x.RoleId)
            .IsRequired();

        builder.Property(x => x.ClaimType)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.ClaimValue)
            .IsRequired()
            .HasMaxLength(1024);

        builder.HasIndex(x => x.RoleId);
    }
}
