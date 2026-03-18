using Cheetah.Core.DataAccess.Attributes;
using Cheetah.Modules.Identity.Domain;
using Crm.Identity.DataAccess.Configurations;
using Crm.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using BaseIdentityDbContext = Cheetah.Modules.Identity.DataAccess.Context.CheetahIdentityDbContext<
    Crm.Identity.DataAccess.IdentityDbContext,
    Crm.Identity.Domain.CrmIdentityUser,
    Crm.Identity.Domain.CrmIdentityRole>;

namespace Crm.Identity.DataAccess;

[ConnectionStringName("Identity")]
public class IdentityDbContext(DbContextOptions<IdentityDbContext> options)
    : BaseIdentityDbContext(options)
{
    protected override IEntityTypeConfiguration<CrmIdentityUser> GetUserConfiguration()
        => new UserConfiguration();

    protected override IEntityTypeConfiguration<CrmIdentityRole> GetRoleConfiguration()
        => new RoleConfiguration();

    protected override IEntityTypeConfiguration<IdentityUserRole<CrmIdentityRole>> GetUserRoleConfiguration()
        => new UserRoleConfiguration();

    protected override IEntityTypeConfiguration<IdentityUserClaim> GetUserClaimConfiguration()
        => new UserClaimConfiguration();

    protected override IEntityTypeConfiguration<IdentityRoleClaim> GetRoleClaimConfiguration()
        => new RoleClaimConfiguration();
}
