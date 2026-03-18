using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Core;
using Cheetah.Modules.Identity.Application.Models;
using Cheetah.Modules.Identity.Application.Queries;
using Crm.Identity.Domain;
using Microsoft.AspNetCore.Identity;

namespace Crm.Identity.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetRoleByIdQuery, RoleModel?>))]
public class GetRoleByIdQueryHandler(RoleManager<CrmIdentityRole> roleManager, IObjectMapper mapper)
    : Cheetah.Modules.Identity.Application.Queries.GetRoleByIdQueryHandler<CrmIdentityRole>(roleManager, mapper)
{
}
