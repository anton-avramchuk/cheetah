using Cheetah.Core.DataAccess.Attributes;
using Cheetah.Core.Identity.Domain;
using Cheetah.Modules.Identity.DataAccess.Configurations;
using Cheetah.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BaseIdentityDbContext = Cheetah.Core.Identity.DataAccess.Context.IdentityDbContext<
    Cheetah.Modules.Identity.DataAccess.IdentityModuleDbContext,
    Cheetah.Modules.Identity.Domain.CrmIdentityUser,
    Cheetah.Modules.Identity.Domain.CrmIdentityRole>;

namespace Cheetah.Modules.Identity.DataAccess;

[ConnectionStringName("Identity")]
public class IdentityModuleDbContext(DbContextOptions<IdentityModuleDbContext> options)
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
