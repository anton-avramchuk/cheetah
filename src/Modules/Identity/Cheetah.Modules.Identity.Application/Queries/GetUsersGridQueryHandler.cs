using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.Grid;
using Cheetah.Modules.Identity.Application.Models;
using Cheetah.Modules.Identity.Domain;

namespace Cheetah.Modules.Identity.Application.Queries;

public abstract class GetUsersGridQueryHandler<TUser, TRole, TUserModel>(IGridRepository<TUser> repository)
    : IQueryHandler<GetUsersGridQuery<TUserModel>, GridResult<TUserModel>>
    where TRole : IdentityRole
    where TUser : IdentityUser<TRole>
    where TUserModel : UserModel
{
    public async ValueTask<GridResult<TUserModel>> HandleAsync(GetUsersGridQuery<TUserModel> gridQuery, CancellationToken ct = default)
    {
        var gridRequest = new GridRequest
        {
            Page = gridQuery.Page,
            PageSize = gridQuery.PageSize,
            Sort = gridQuery.Sort,
            Filter = gridQuery.Filter
        };
        return await repository.GetGridAsync<TUserModel>(gridRequest, ct);
    }
}
