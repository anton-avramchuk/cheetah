using Cheetah.Modules.Identity.DataAccess.Configurations;
using Cheetah.Core.Identity.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Identity.DataAccess.Configurations;

public class UserClaimConfigurationOptions : IdentityUserClaimConfigurationOptions
{
    public override string Schema => "identity";
}

public class UserClaimConfiguration : IdentityUserClaimConfiguration<UserClaimConfigurationOptions>
{
    protected override UserClaimConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<IdentityUserClaim> builder)
    {
        base.Configure(builder);
    }
}
