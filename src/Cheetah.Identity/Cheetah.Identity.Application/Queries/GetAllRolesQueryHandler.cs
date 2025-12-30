using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;
using Cheetah.Identity.DataAccess;
using Cheetah.Identity.Shared.ViewModels;

namespace Cheetah.Identity.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllRolesQuery, List<RoleViewModel>>))]
public class GetAllRolesQueryHandler : IQueryHandler<GetAllRolesQuery, List<RoleViewModel>>
{
    private readonly IIdentityDbContext _dbContext;

    public GetAllRolesQueryHandler(IIdentityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async ValueTask<List<RoleViewModel>> HandleAsync(GetAllRolesQuery query, CancellationToken ct)
    {
        return await _dbContext.Roles
            .AsNoTracking()
            .Select(r => new RoleViewModel
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(ct);
    }
}
