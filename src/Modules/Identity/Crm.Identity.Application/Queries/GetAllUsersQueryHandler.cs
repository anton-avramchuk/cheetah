using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Crm.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Crm.Identity.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllUsersQuery, IReadOnlyList<UserModel>>))]
public class GetAllUsersQueryHandler(UserManager<CrmUser> userManager)
    : IQueryHandler<GetAllUsersQuery, IReadOnlyList<UserModel>>
{
    public async ValueTask<IReadOnlyList<UserModel>> HandleAsync(GetAllUsersQuery query, CancellationToken ct = default)
    {
        return await userManager.Users
            .AsNoTracking()
            .OrderBy(u => u.UserName)
            .Select(u => new UserModel(u.Id, u.UserName!, u.Email!, u.EmailConfirmed))
            .ToListAsync(ct);
    }
}
