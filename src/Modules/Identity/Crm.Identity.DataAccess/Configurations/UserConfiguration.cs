using Cheetah.Core.Identity.DataAccess.Configurations;
using Crm.Identity.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Identity.DataAccess.Configurations;

public class UserConfigurationOptions : IdentityUserConfigurationOptions<CrmUser, CrmRole>
{
    public override string Schema => "identity";
}

public class UserConfiguration : IdentityUserConfiguration<CrmUser, CrmRole, UserConfigurationOptions>
{
    protected override UserConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<CrmUser> builder)
    {
        base.Configure(builder);
    }
}
