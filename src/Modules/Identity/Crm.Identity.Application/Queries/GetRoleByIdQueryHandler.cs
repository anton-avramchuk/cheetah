using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Crm.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Crm.Identity.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetRoleByIdQuery, RoleModel?>))]
public class GetRoleByIdQueryHandler(RoleManager<CrmRole> roleManager)
    : IQueryHandler<GetRoleByIdQuery, RoleModel?>
{
    public async ValueTask<RoleModel?> HandleAsync(GetRoleByIdQuery query, CancellationToken ct = default)
    {
        var role = await roleManager.Roles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == query.Id, ct);

        return role is null ? null : new RoleModel(role.Id, role.Name!);
    }
}
