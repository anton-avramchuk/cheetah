using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Crm.Identity.Domain;

namespace Crm.Identity.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllRolesQuery, GridResult<RoleModel>>))]
public class GetAllRolesQueryHandler(IGridRepository<CrmRole> repository)
    : IQueryHandler<GetAllRolesQuery, GridResult<RoleModel>>
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
