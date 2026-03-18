using Cheetah.Modules.Identity.Application.Queries;
using Crm.Identity.Contracts.Requests;

namespace Crm.Identity.Api.Endpoints;

public sealed class GetRoleByIdEndpoint
    : Cheetah.Modules.Identity.Api.Endpoints.GetRoleByIdEndpoint<GetRoleByIdRequest, GetRoleByIdQuery>
{
}
