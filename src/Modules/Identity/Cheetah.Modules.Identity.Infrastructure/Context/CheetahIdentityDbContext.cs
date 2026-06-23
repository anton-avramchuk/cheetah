using Cheetah.Core.EntityFramework;
using Cheetah.Modules.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Modules.Identity.Infrastructure.Context;

public abstract class CheetahIdentityDbContext<TDbContext, TIdentityUser, TIdentityRole>(
    DbContextOptions<TDbContext> options) : CrmDbContext<TDbContext>(options), IIdentityDbContext
    where TDbContext : DbContext
    where TIdentityUser : IdentityUser<TIdentityRole>
    where TIdentityRole : IdentityRole
{
    protected abstract IEntityTypeConfiguration<TIdentityUser> GetUserConfiguration();
    protected abstract IEntityTypeConfiguration<TIdentityRole> GetRoleConfiguration();
    protected abstract IEntityTypeConfiguration<IdentityUserRole<TIdentityRole>> GetUserRoleConfiguration();
    protected abstract IEntityTypeConfiguration<IdentityUserClaim> GetUserClaimConfiguration();
    protected abstract IEntityTypeConfiguration<IdentityRoleClaim> GetRoleClaimConfiguration();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(GetUserConfiguration());
        modelBuilder.ApplyConfiguration(GetRoleConfiguration());
        modelBuilder.ApplyConfiguration(GetUserRoleConfiguration());
        modelBuilder.ApplyConfiguration(GetUserClaimConfiguration());
        modelBuilder.ApplyConfiguration(GetRoleClaimConfiguration());
    }
}
