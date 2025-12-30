using Cheetah.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Identity.DataAccess;

/// <summary>
/// Interface for Identity DbContext
/// </summary>
public interface IIdentityDbContext
{
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<UserClaim> UserClaims { get; }
    DbSet<RoleClaim> RoleClaims { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
