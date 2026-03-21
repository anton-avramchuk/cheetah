using Cheetah.Modules.Identity.DataAccess.Configurations;
using AppName.Identity.DataAccess;
using Cheetah.Modules.Identity.Domain;
using AppName.Identity.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppName.Identity.DataAccess.Configurations;

public class UserRoleConfigurationOptions : IdentityUserRoleConfigurationOptions<AppNameIdentityRole>
{
    public override string Schema => "identity";
}

public class UserRoleConfiguration : IdentityUserRoleConfiguration<AppNameIdentityRole, UserRoleConfigurationOptions>
{
    protected override UserRoleConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<IdentityUserRole<AppNameIdentityRole>> builder)
    {
        base.Configure(builder);
    }
}
