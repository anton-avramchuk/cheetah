using Cheetah.Modules.Identity.Application.Models;
using Cheetah.Modules.Identity.Application.Queries;
using Cheetah.Modules.Identity.Contracts.Requests;
using Cheetah.Modules.Identity.Contracts.Response;

namespace AppName.Identity.Api.Endpoints;

public sealed class GetRolesGridEndpoint
    : Cheetah.Modules.Identity.Api.Endpoints.GetRolesGridEndpoint<GetRolesGridRequest, GetRolesGridQuery<RoleModel>, RoleModel, RoleViewModel>
{
}
