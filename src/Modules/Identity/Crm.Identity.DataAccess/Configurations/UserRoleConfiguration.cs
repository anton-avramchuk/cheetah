using Cheetah.Core.Identity.DataAccess.Configurations;
using Cheetah.Core.Identity.Domain;
using Crm.Identity.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Identity.DataAccess.Configurations;

public class UserRoleConfigurationOptions : IdentityUserRoleConfigurationOptions<CrmRole>
{
    public override string Schema => "identity";
}

public class UserRoleConfiguration : IdentityUserRoleConfiguration<CrmRole, UserRoleConfigurationOptions>
{
    protected override UserRoleConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<IdentityUserRole<CrmRole>> builder)
    {
        base.Configure(builder);
    }
}
