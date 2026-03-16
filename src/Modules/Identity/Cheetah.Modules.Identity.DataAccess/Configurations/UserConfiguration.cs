using Cheetah.Core.Identity.DataAccess.Configurations;
using Cheetah.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.Identity.DataAccess.Configurations;

public class UserConfigurationOptions : IdentityUserConfigurationOptions<CrmIdentityUser, CrmIdentityRole>
{
    public override string Schema => "identity";
}

public class UserConfiguration : IdentityUserConfiguration<CrmIdentityUser, CrmIdentityRole, UserConfigurationOptions>
{
    protected override UserConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<CrmIdentityUser> builder)
    {
        base.Configure(builder);
    }
}
