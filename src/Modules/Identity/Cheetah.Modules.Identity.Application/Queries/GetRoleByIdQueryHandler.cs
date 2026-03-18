using Cheetah.Core.CQRS;
using Cheetah.Mapping.Core;
using Cheetah.Modules.Identity.Application.Models;
using Cheetah.Modules.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Modules.Identity.Application.Queries;

public abstract class GetRoleByIdQueryHandler<TRole>(RoleManager<TRole> roleManager, IObjectMapper mapper)
    : IQueryHandler<GetRoleByIdQuery, RoleModel?>
    where TRole : IdentityRole
{
    public async ValueTask<RoleModel?> HandleAsync(GetRoleByIdQuery query, CancellationToken ct = default)
    {
        return await mapper
            .ProjectTo<RoleModel>(roleManager.Roles.AsNoTracking().Where(r => r.Id == query.Id))
            .FirstOrDefaultAsync(ct);
    }
}
