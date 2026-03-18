using Cheetah.Modules.Identity.Application.Queries;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Core;
using Cheetah.Modules.Identity.Application.Models;
using Cheetah.Modules.Identity.Domain;
using Crm.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Crm.Identity.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetUserByIdQuery, UserDetailModel?>))]
public class GetUserByIdQueryHandler(UserManager<CrmIdentityUser> userManager, IObjectMapper mapper)
    : IQueryHandler<GetUserByIdQuery, UserDetailModel?>
{
    public async ValueTask<UserDetailModel?> HandleAsync(GetUserByIdQuery query, CancellationToken ct = default)
    {
        return await mapper
            .ProjectTo<UserDetailModel>(userManager.Users.AsNoTracking().Where(u => u.Id == query.Id))
            .FirstOrDefaultAsync(ct);
    }
}
