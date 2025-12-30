using Cheetah.Core.DependencyInjection;
using Cheetah.Core.EntityFramework;
using Cheetah.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Identity.DataAccess;

/// <summary>
/// Identity database context
/// </summary>
[Export(LifetimeType.Scoped, typeof(ICrmDbContext), typeof(IIdentityDbContext), typeof(IdentityDbContext))]
public class IdentityDbContext : CrmDbContext<IdentityDbContext>, IIdentityDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<UserClaim> UserClaims => Set<UserClaim>();
    public DbSet<RoleClaim> RoleClaims => Set<RoleClaim>();

    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IdentityDbContext).Assembly);
    }
}
