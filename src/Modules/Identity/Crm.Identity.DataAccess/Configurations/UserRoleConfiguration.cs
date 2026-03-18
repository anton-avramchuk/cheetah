using Cheetah.Modules.Identity.DataAccess.Configurations;
using Crm.Identity.DataAccess;
using Cheetah.Modules.Identity.Domain;
using Crm.Identity.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Identity.DataAccess.Configurations;

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
