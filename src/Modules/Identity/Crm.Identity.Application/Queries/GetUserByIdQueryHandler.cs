using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Crm.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Crm.Identity.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetUserByIdQuery, UserModel?>))]
public class GetUserByIdQueryHandler(UserManager<CrmUser> userManager)
    : IQueryHandler<GetUserByIdQuery, UserModel?>
{
    public async ValueTask<UserModel?> HandleAsync(GetUserByIdQuery query, CancellationToken ct = default)
    {
        var user = await userManager.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == query.Id, ct);

        return user is null ? null : new UserModel(user.Id, user.UserName!, user.Email!, user.EmailConfirmed);
    }
}
