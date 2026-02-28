using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Crm.Identity.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Crm.Identity.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllRolesQuery, GridResult<RoleModel>>))]
public class GetAllRolesQueryHandler(RoleManager<CrmRole> roleManager)
    : IQueryHandler<GetAllRolesQuery, GridResult<RoleModel>>
{
    public async ValueTask<GridResult<RoleModel>> HandleAsync(GetAllRolesQuery query, CancellationToken ct = default)
    {
        var queryable = roleManager.Roles.AsNoTracking();
        var total = await queryable.CountAsync(ct);

        var items = await queryable
            .OrderBy(r => r.Name)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize > 0 ? query.PageSize : int.MaxValue)
            .Select(r => new RoleModel(r.Id, r.Name!))
            .ToListAsync(ct);

        return new GridResult<RoleModel>(items, total);
    }
}
