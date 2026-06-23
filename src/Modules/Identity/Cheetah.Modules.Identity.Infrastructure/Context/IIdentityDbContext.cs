using Cheetah.Core.EntityFramework;

namespace Cheetah.Modules.Identity.Infrastructure.Context;

/// <summary>
/// Marker interface for identity DbContext instances.
/// Used as a generic constraint in <see cref="Services.IdentityUserStore{TIdentityUser,TIdentityRole,TIdentityContext}"/>
/// and <see cref="Services.IdentityRoleStore{TIdentityRole,TIdentityDbContext}"/> to ensure type safety
/// without coupling stores to a concrete DbContext implementation.
/// </summary>
public interface IIdentityDbContext : ICrmDbContext
{
}
