using Cheetah.Core.Identity.DataAccess.Configurations;
using Crm.Identity.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Crm.Identity.DataAccess.Configurations;

public class RoleConfigurationOptions : IdentityRoleConfigurationOptions<CrmRole>
{
    public override string Schema => "identity";
}

public class RoleConfiguration : IdentityRoleConfiguration<CrmRole, RoleConfigurationOptions>
{
    protected override RoleConfigurationOptions Options { get; } = new();

    public override void Configure(EntityTypeBuilder<CrmRole> builder)
    {
        base.Configure(builder);
    }
}
