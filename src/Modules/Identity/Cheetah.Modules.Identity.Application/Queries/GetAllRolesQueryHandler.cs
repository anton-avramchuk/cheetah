using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.Grid;
using Cheetah.Modules.Identity.Application.Models;
using Cheetah.Modules.Identity.Domain;

namespace Cheetah.Modules.Identity.Application.Queries;

public abstract class GetAllRolesQueryHandler<TRole>(IGridRepository<TRole> repository)
    : IQueryHandler<GetAllRolesQuery, GridResult<RoleModel>>
    where TRole : IdentityRole
{
    public async ValueTask<GridResult<RoleModel>> HandleAsync(GetAllRolesQuery query, CancellationToken ct = default)
    {
        var gridRequest = new GridRequest
        {
            Page = query.Page,
            PageSize = query.PageSize,
            Sort = query.Sort,
            Filter = query.Filter
        };
        return await repository.GetGridAsync<RoleModel>(gridRequest, ct);
    }
}
