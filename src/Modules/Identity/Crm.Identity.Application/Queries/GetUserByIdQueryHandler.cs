using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Core;
using Cheetah.Modules.Identity.Application.Models;
using Cheetah.Modules.Identity.Application.Queries;
using Crm.Identity.Domain;
using Microsoft.AspNetCore.Identity;

namespace Crm.Identity.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetUserByIdQuery, UserDetailModel?>))]
public class GetUserByIdQueryHandler(UserManager<CrmIdentityUser> userManager, IObjectMapper mapper)
    : Cheetah.Modules.Identity.Application.Queries.GetUserByIdQueryHandler<CrmIdentityUser, CrmIdentityRole>(userManager, mapper)
{
}
