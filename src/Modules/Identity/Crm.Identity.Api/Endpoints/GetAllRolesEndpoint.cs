using Cheetah.Modules.Identity.Application.Queries;
using Crm.Identity.Contracts.Requests;

namespace Crm.Identity.Api.Endpoints;

public sealed class GetAllRolesEndpoint
    : Cheetah.Modules.Identity.Api.Endpoints.GetAllRolesEndpoint<GetAllRolesRequest, GetAllRolesQuery>
{
}
