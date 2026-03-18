using AppName.Identity.Domain;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Core;
using Cheetah.Modules.Identity.Application.Models;
using Cheetah.Modules.Identity.Application.Queries;
using Microsoft.AspNetCore.Identity;

namespace AppName.Identity.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetUserByIdQuery, UserDetailModel?>))]
public class GetUserByIdQueryHandler(UserManager<AppNameIdentityUser> userManager, IObjectMapper mapper)
    : Cheetah.Modules.Identity.Application.Queries.GetUserByIdQueryHandler<AppNameIdentityUser, AppNameIdentityRole>(userManager, mapper)
{
}
