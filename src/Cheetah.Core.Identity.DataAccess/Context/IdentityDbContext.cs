using Cheetah.Core.EntityFramework;
using Cheetah.Core.Identity.Domain;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Core.Identity.DataAccess.Context;

public abstract class IdentityDbContext<TDbContext, TIdentityUser, TIdentityRole> : CrmDbContext<TDbContext>, IIdentityDbContext
    where TDbContext : DbContext
    where TIdentityUser : IdentityUser<TIdentityRole>
    where TIdentityRole : IdentityRole
{
    protected IdentityDbContext(DbContextOptions<TDbContext> options) : base(options)
    {
    }

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