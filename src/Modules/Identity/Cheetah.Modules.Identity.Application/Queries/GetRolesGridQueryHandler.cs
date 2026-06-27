using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.Grid;
using Cheetah.Modules.Identity.Application.Models;
using Cheetah.Modules.Identity.Domain;

namespace Cheetah.Modules.Identity.Application.Queries;

public sealed class GetRolesGridQueryHandler<TRole, TRoleModel>(IGridRepository<TRole> repository)
    : IQueryHandler<GetRolesGridQuery<TRoleModel>, GridResult<TRoleModel>>
    where TRole : IdentityRole
    where TRoleModel : RoleModel
{
    public async ValueTask<GridResult<TRoleModel>> HandleAsync(GetRolesGridQuery<TRoleModel> gridQuery, CancellationToken ct = default)
    {
        var gridRequest = new GridRequest
        {
            Page = gridQuery.Page,
            PageSize = gridQuery.PageSize,
            Sort = gridQuery.Sort,
            Filter = gridQuery.Filter
        };
        return await repository.GetGridAsync<TRoleModel>(gridRequest, ct);
    }
}
