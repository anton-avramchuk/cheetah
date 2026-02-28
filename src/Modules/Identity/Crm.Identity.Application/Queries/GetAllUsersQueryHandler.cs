using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Crm.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Crm.Identity.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllUsersQuery, GridResult<UserModel>>))]
public class GetAllUsersQueryHandler(UserManager<CrmUser> userManager)
    : IQueryHandler<GetAllUsersQuery, GridResult<UserModel>>
{
    public async ValueTask<GridResult<UserModel>> HandleAsync(GetAllUsersQuery query, CancellationToken ct = default)
    {
        var queryable = userManager.Users.AsNoTracking();
        var total = await queryable.CountAsync(ct);

        var items = await queryable
            .OrderBy(u => u.UserName)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize > 0 ? query.PageSize : int.MaxValue)
            .Select(u => new UserModel(u.Id, u.UserName!, u.Email!, u.EmailConfirmed))
            .ToListAsync(ct);

        return new GridResult<UserModel>(items, total);
    }
}
