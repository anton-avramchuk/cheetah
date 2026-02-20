using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Crm.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Crm.Identity.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllRolesQuery, IReadOnlyList<RoleModel>>))]
public class GetAllRolesQueryHandler(RoleManager<CrmRole> roleManager)
    : IQueryHandler<GetAllRolesQuery, IReadOnlyList<RoleModel>>
{
    public async ValueTask<IReadOnlyList<RoleModel>> HandleAsync(GetAllRolesQuery query, CancellationToken ct = default)
    {
        return await roleManager.Roles
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new RoleModel(r.Id, r.Name!))
            .ToListAsync(ct);
    }
}
