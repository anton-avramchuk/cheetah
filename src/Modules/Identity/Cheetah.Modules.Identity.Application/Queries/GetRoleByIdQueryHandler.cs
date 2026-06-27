using Cheetah.Core.CQRS;
using Cheetah.Mapping.Core;
using Cheetah.Modules.Identity.Application.Models;
using Cheetah.Modules.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Modules.Identity.Application.Queries;

public sealed class GetRoleByIdQueryHandler<TRole, TRoleModel>(RoleManager<TRole> roleManager, IObjectMapper mapper)
    : IQueryHandler<GetRoleByIdQuery<TRoleModel>, TRoleModel?>
    where TRole : IdentityRole
    where TRoleModel : RoleModel
{
    public async ValueTask<TRoleModel?> HandleAsync(GetRoleByIdQuery<TRoleModel> query, CancellationToken ct = default)
    {
        return await mapper
            .ProjectTo<TRoleModel>(roleManager.Roles.AsNoTracking().Where(r => r.Id == query.Id))
            .FirstOrDefaultAsync(ct);
    }
}
