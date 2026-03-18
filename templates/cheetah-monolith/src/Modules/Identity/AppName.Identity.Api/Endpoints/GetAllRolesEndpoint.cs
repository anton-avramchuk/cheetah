using Cheetah.Modules.Identity.Application.Queries;
using Cheetah.Modules.Identity.Contracts.Requests;

namespace AppName.Identity.Api.Endpoints;

public sealed class GetAllRolesEndpoint
    : Cheetah.Modules.Identity.Api.Endpoints.GetAllRolesEndpoint<GetAllRolesRequest, GetAllRolesQuery>
{
}
