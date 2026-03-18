using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.Grid;
using Cheetah.Modules.Identity.Application.Models;
using Cheetah.Modules.Identity.Domain;

namespace Cheetah.Modules.Identity.Application.Queries;

public abstract class GetAllUsersQueryHandler<TUser, TRole>(IGridRepository<TUser> repository)
    : IQueryHandler<GetAllUsersQuery, GridResult<UserModel>>
    where TRole : IdentityRole
    where TUser : IdentityUser<TRole>
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
