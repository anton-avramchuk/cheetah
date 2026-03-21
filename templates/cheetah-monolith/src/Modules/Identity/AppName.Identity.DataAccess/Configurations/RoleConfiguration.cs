using Cheetah.Modules.Identity.DataAccess.Configurations;
using Cheetah.Modules.Identity.Domain;
using AppName.Identity.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppName.Identity.DataAccess.Configurations;

public class RoleConfigurationOptions : IdentityRoleConfigurationOptions<AppNameIdentityRole>
{
    public override string Schema => "identity";
}

public class RoleConfiguration : IdentityRoleConfiguration<AppNameIdentityRole, RoleConfigurationOptions>
{
    protected override RoleConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<AppNameIdentityRole> builder)
    {
        base.Configure(builder);
    }
}
