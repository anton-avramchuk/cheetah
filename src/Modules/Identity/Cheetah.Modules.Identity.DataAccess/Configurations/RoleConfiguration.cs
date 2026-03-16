using Cheetah.Core.Identity.DataAccess.Configurations;
using Cheetah.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cheetah.Modules.Identity.DataAccess.Configurations;

public class RoleConfigurationOptions : IdentityRoleConfigurationOptions<CrmIdentityRole>
{
    public override string Schema => "identity";
}

public class RoleConfiguration : IdentityRoleConfiguration<CrmIdentityRole, RoleConfigurationOptions>
{
    protected override RoleConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<CrmIdentityRole> builder)
    {
        base.Configure(builder);
    }
}
