using AppName.Identity.Domain;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Cheetah.Modules.Identity.Application.Models;
using Cheetah.Modules.Identity.Application.Queries;

namespace AppName.Identity.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetRolesGridQuery<RoleModel>, GridResult<RoleModel>>))]
public class GetRolesGridQueryHandler(IGridRepository<AppNameIdentityRole> repository)
    : Cheetah.Modules.Identity.Application.Queries.GetRolesGridQueryHandler<AppNameIdentityRole, RoleModel>(repository)
{
}
