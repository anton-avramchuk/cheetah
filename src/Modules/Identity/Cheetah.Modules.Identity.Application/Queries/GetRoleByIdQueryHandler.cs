using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Core;
using Cheetah.Modules.Identity.Application.Models;
using Cheetah.Modules.Identity.Domain;
using Crm.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Cheetah.Modules.Identity.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetRoleByIdQuery, RoleModel?>))]
public class GetRoleByIdQueryHandler(RoleManager<CrmIdentityRole> roleManager, IObjectMapper mapper)
    : IQueryHandler<GetRoleByIdQuery, RoleModel?>
{
    public async ValueTask<RoleModel?> HandleAsync(GetRoleByIdQuery query, CancellationToken ct = default)
    {
        return await mapper
            .ProjectTo<RoleModel>(roleManager.Roles.AsNoTracking().Where(r => r.Id == query.Id))
            .FirstOrDefaultAsync(ct);
    }
}
