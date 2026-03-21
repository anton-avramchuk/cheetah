using Cheetah.Modules.Identity.DataAccess.Configurations;
using Cheetah.Modules.Identity.Domain;
using AppName.Identity.DataAccess;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppName.Identity.DataAccess.Configurations;

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
