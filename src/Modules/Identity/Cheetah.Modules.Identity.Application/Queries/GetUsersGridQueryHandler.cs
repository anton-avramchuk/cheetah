using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.Grid;
using Cheetah.Modules.Identity.Application.Models;
using Cheetah.Modules.Identity.Domain;

namespace Cheetah.Modules.Identity.Application.Queries;

public abstract class GetUsersGridQueryHandler<TUser, TRole>(IGridRepository<TUser> repository)
    : IQueryHandler<GetUsersGridQuery, GridResult<UserModel>>
    where TRole : IdentityRole
    where TUser : IdentityUser<TRole>
{
    public async ValueTask<GridResult<UserModel>> HandleAsync(GetUsersGridQuery gridQuery, CancellationToken ct = default)
    {
        var gridRequest = new GridRequest
        {
            Page = gridQuery.Page,
            PageSize = gridQuery.PageSize,
            Sort = gridQuery.Sort,
            Filter = gridQuery.Filter
        };
        return await repository.GetGridAsync<UserModel>(gridRequest, ct);
    }
}
