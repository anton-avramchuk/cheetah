using Cheetah.Modules.Identity.Application.Models;
using Cheetah.Modules.Identity.Application.Queries;
using Crm.Identity.Contracts.Requests;
using Crm.Identity.Contracts.Response;

namespace Crm.Identity.Api.Endpoints;

public sealed class GetRolesGridEndpoint
    : Cheetah.Modules.Identity.Api.Endpoints.GetRolesGridEndpoint<GetRolesGridRequest, GetRolesGridQuery<RoleModel>, RoleModel, RoleViewModel>
{
}
