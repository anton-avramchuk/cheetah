using Cheetah.Core.CQRS;
using Cheetah.Mapping.Core;
using Cheetah.Modules.Identity.Application.Models;
using Cheetah.Modules.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Modules.Identity.Application.Queries;

public sealed class GetUserByIdQueryHandler<TUser, TRole, TUserDetailModel>(UserManager<TUser> userManager, IObjectMapper mapper)
    : IQueryHandler<GetUserByIdQuery<TUserDetailModel>, TUserDetailModel?>
    where TRole : IdentityRole
    where TUser : IdentityUser<TRole>
    where TUserDetailModel : UserDetailModel
{
    public async ValueTask<TUserDetailModel?> HandleAsync(GetUserByIdQuery<TUserDetailModel> query, CancellationToken ct = default)
    {
        return await mapper
            .ProjectTo<TUserDetailModel>(userManager.Users.AsNoTracking().Where(u => u.Id == query.Id))
            .FirstOrDefaultAsync(ct);
    }
}
