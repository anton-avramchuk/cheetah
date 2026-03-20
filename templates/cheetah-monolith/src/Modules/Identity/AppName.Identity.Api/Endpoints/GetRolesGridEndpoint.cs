using AppName.Identity.Contracts.Requests;
using AppName.Identity.Contracts.Response;
using Cheetah.Modules.Identity.Application.Models;
using Cheetah.Modules.Identity.Application.Queries;

namespace AppName.Identity.Api.Endpoints;

public sealed class GetRolesGridEndpoint
    : Cheetah.Modules.Identity.Api.Endpoints.GetRolesGridEndpoint<GetRolesGridRequest, GetRolesGridQuery<RoleModel>, RoleModel, RoleViewModel>
{
}
