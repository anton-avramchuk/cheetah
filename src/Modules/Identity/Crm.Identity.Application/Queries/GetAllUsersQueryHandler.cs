using Cheetah.Modules.Identity.Application.Queries;
using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Cheetah.Modules.Identity.Application.Models;
using Cheetah.Modules.Identity.Domain;
using Crm.Identity.Domain;

namespace Crm.Identity.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllUsersQuery, GridResult<UserModel>>))]
public class GetAllUsersQueryHandler(IGridRepository<CrmIdentityUser> repository)
    : IQueryHandler<GetAllUsersQuery, GridResult<UserModel>>
{
    public async ValueTask<GridResult<UserModel>> HandleAsync(GetAllUsersQuery query, CancellationToken ct = default)
    {
        var gridRequest = new GridRequest
        {
            Page = query.Page,
            PageSize = query.PageSize,
            Sort = query.Sort,
            Filter = query.Filter
        };
        return await repository.GetGridAsync<UserModel>(gridRequest, ct);
    }
}
