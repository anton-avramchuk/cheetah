using Cheetah.Core.DataAccess.Attributes;
using Cheetah.Modules.Identity.Domain;
using AppName.Identity.DataAccess.Configurations;
using AppName.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using BaseIdentityDbContext = Cheetah.Modules.Identity.DataAccess.Context.CheetahIdentityDbContext<
    AppName.Identity.DataAccess.IdentityDbContext,
    AppName.Identity.Domain.AppNameIdentityUser,
    AppName.Identity.Domain.AppNameIdentityRole>;

namespace AppName.Identity.DataAccess;

[ConnectionStringName("Identity")]
public class IdentityDbContext(DbContextOptions<IdentityDbContext> options)
    : BaseIdentityDbContext(options)
{
    protected override IEntityTypeConfiguration<AppNameIdentityUser> GetUserConfiguration()
        => new UserConfiguration();

    protected override IEntityTypeConfiguration<AppNameIdentityRole> GetRoleConfiguration()
        => new RoleConfiguration();

    protected override IEntityTypeConfiguration<IdentityUserRole<AppNameIdentityRole>> GetUserRoleConfiguration()
        => new UserRoleConfiguration();

    protected override IEntityTypeConfiguration<IdentityUserClaim> GetUserClaimConfiguration()
        => new UserClaimConfiguration();

    protected override IEntityTypeConfiguration<IdentityRoleClaim> GetRoleClaimConfiguration()
        => new RoleClaimConfiguration();
}
