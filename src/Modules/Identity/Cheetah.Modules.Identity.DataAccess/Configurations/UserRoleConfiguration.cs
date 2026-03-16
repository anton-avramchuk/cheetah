using Cheetah.Core.Identity.DataAccess.Configurations;
using Cheetah.Core.Identity.Domain;
using Cheetah.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.Identity.DataAccess.Configurations;

public class UserRoleConfigurationOptions : IdentityUserRoleConfigurationOptions<CrmIdentityRole>
{
    public override string Schema => "identity";
}

public class UserRoleConfiguration : IdentityUserRoleConfiguration<CrmIdentityRole, UserRoleConfigurationOptions>
{
    protected override UserRoleConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<IdentityUserRole<CrmIdentityRole>> builder)
    {
        base.Configure(builder);
    }
}
