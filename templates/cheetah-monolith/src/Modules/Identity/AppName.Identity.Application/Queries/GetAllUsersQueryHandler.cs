using AppName.Identity.Domain;
using Cheetah.Contracts.Responses;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Grid;
using Cheetah.Modules.Identity.Application.Models;
using Cheetah.Modules.Identity.Application.Queries;

namespace AppName.Identity.Application.Queries;

[Export(LifetimeType.Scoped, typeof(IQueryHandler<GetAllUsersQuery, GridResult<UserModel>>))]
public class GetAllUsersQueryHandler(IGridRepository<AppNameIdentityUser> repository)
    : Cheetah.Modules.Identity.Application.Queries.GetAllUsersQueryHandler<AppNameIdentityUser, AppNameIdentityRole>(repository)
{
}
