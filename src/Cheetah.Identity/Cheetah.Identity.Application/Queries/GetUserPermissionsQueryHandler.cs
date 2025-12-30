using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;
using Cheetah.Identity.DataAccess;

namespace Cheetah.Identity.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetUserPermissionsQuery, List<string>>))]
public class GetUserPermissionsQueryHandler : IQueryHandler<GetUserPermissionsQuery, List<string>>
{
    private readonly IIdentityDbContext _dbContext;

    public GetUserPermissionsQueryHandler(IIdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<List<string>> HandleAsync(GetUserPermissionsQuery query, CancellationToken ct)
    {
        // Get user with roles
        var user = await _dbContext.Users
            .AsNoTracking()
            .Include(u => u.Roles)
            .Include(u => u.Claims)
            .FirstOrDefaultAsync(u => u.Id == query.UserId, ct);

        if (user == null)
            return [];

        var permissions = new HashSet<string>();

        // Add permissions from roles (RoleClaims)
        var roleIds = user.Roles.Select(ur => ur.RoleId).ToList();
        if (roleIds.Any())
        {
            var roleClaims = await _dbContext.RoleClaims
                .AsNoTracking()
                .Where(rc => roleIds.Contains(rc.RoleId) && rc.ClaimType == "Permission")
                .Select(rc => rc.ClaimValue)
                .ToListAsync(ct);

            foreach (var claim in roleClaims)
            {
                permissions.Add(claim);
            }
        }

        // Add personal permissions from UserClaims
        var userClaims = user.Claims
            .Where(uc => uc.ClaimType == "Permission")
            .Select(uc => uc.ClaimValue);

        foreach (var claim in userClaims)
        {
            permissions.Add(claim);
        }

        return permissions.ToList();
    }
}
