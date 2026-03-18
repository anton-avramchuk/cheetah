using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.Grid;
using Cheetah.Modules.Identity.Application.Models;
using Cheetah.Modules.Identity.Domain;

namespace Cheetah.Modules.Identity.Application.Queries;

public abstract class GetRolesGridQueryHandler<TRole>(IGridRepository<TRole> repository)
    : IQueryHandler<GetRolesGridQuery, GridResult<RoleModel>>
    where TRole : IdentityRole
{
    public async ValueTask<GridResult<RoleModel>> HandleAsync(GetRolesGridQuery gridQuery, CancellationToken ct = default)
    {
        var gridRequest = new GridRequest
        {
            Page = gridQuery.Page,
            PageSize = gridQuery.PageSize,
            Sort = gridQuery.Sort,
            Filter = gridQuery.Filter
        };
        return await repository.GetGridAsync<RoleModel>(gridRequest, ct);
    }
}
