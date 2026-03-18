using AppName.Identity.Domain;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Core;
using Cheetah.Modules.Identity.Application.Models;
using Cheetah.Modules.Identity.Application.Queries;
using Microsoft.AspNetCore.Identity;

namespace AppName.Identity.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetRoleByIdQuery, RoleModel?>))]
public class GetRoleByIdQueryHandler(RoleManager<AppNameIdentityRole> roleManager, IObjectMapper mapper)
    : Cheetah.Modules.Identity.Application.Queries.GetRoleByIdQueryHandler<AppNameIdentityRole>(roleManager, mapper)
{
}
