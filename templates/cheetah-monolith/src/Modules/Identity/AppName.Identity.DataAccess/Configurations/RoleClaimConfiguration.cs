using Cheetah.Modules.Identity.DataAccess.Configurations;
using Cheetah.Modules.Identity.Domain;
using AppName.Identity.DataAccess;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppName.Identity.DataAccess.Configurations;

public class RoleClaimConfigurationOptions : IdentityRoleClaimConfigurationOptions
{
    public override string Schema => "identity";
}

public class RoleClaimConfiguration : IdentityRoleClaimConfiguration<RoleClaimConfigurationOptions>
{
    protected override RoleClaimConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<IdentityRoleClaim> builder)
    {
        base.Configure(builder);
    }
}
