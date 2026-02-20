using Cheetah.Core.DataAccess.Attributes;
using Cheetah.Core.Identity.Domain;
using Crm.Identity.DataAccess.Configurations;
using Crm.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BaseIdentityDbContext = Cheetah.Core.Identity.DataAccess.Context.IdentityDbContext<
    Crm.Identity.DataAccess.IdentityDbContext,
    Crm.Identity.Domain.CrmUser,
    Crm.Identity.Domain.CrmRole>;

namespace Crm.Identity.DataAccess;

[ConnectionStringName("Identity")]
public class IdentityDbContext(DbContextOptions<IdentityDbContext> options)
    : BaseIdentityDbContext(options)
{
    protected override IEntityTypeConfiguration<CrmUser> GetUserConfiguration()
        => new UserConfiguration();

    protected override IEntityTypeConfiguration<CrmRole> GetRoleConfiguration()
        => new RoleConfiguration();

    protected override IEntityTypeConfiguration<IdentityUserRole<CrmRole>> GetUserRoleConfiguration()
        => new UserRoleConfiguration();

    protected override IEntityTypeConfiguration<IdentityUserClaim> GetUserClaimConfiguration()
        => new UserClaimConfiguration();

    protected override IEntityTypeConfiguration<IdentityRoleClaim> GetRoleClaimConfiguration()
        => new RoleClaimConfiguration();
}
