using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Cheetah.Modules.Identity.Application.Models;
using Cheetah.Modules.Identity.Application.Queries;
using Cheetah.Contracts.Responses;
using Crm.Identity.Domain;

namespace Crm.Identity.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetRolesGridQuery, GridResult<RoleModel>>))]
public class GetRolesGridQueryHandler(IGridRepository<CrmIdentityRole> repository)
    : Cheetah.Modules.Identity.Application.Queries.GetRolesGridQueryHandler<CrmIdentityRole>(repository)
{
}
