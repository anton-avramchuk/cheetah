using Cheetah.Core.CQRS;
using Cheetah.Mapping.Core;
using Cheetah.Modules.Identity.Application.Models;
using Cheetah.Modules.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Modules.Identity.Application.Queries;

public abstract class GetUserByIdQueryHandler<TUser, TRole>(UserManager<TUser> userManager, IObjectMapper mapper)
    : IQueryHandler<GetUserByIdQuery, UserDetailModel?>
    where TRole : IdentityRole
    where TUser : IdentityUser<TRole>
{
    public async ValueTask<UserDetailModel?> HandleAsync(GetUserByIdQuery query, CancellationToken ct = default)
    {
        return await mapper
            .ProjectTo<UserDetailModel>(userManager.Users.AsNoTracking().Where(u => u.Id == query.Id))
            .FirstOrDefaultAsync(ct);
    }
}
