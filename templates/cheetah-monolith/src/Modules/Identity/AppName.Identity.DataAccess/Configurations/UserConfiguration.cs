using Cheetah.Modules.Identity.DataAccess.Configurations;
using Cheetah.Modules.Identity.Domain;
using AppName.Identity.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppName.Identity.DataAccess.Configurations;

public class UserConfigurationOptions : IdentityUserConfigurationOptions<AppNameIdentityUser, AppNameIdentityRole>
{
    public override string Schema => "identity";
}

public class UserConfiguration : IdentityUserConfiguration<AppNameIdentityUser, AppNameIdentityRole, UserConfigurationOptions>
{
    protected override UserConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<AppNameIdentityUser> builder)
    {
        base.Configure(builder);
    }
}
