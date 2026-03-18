using Cheetah.Modules.Identity.Application.Queries;
using Cheetah.Modules.Identity.Contracts.Requests;

namespace Crm.Identity.Api.Endpoints;

public sealed class GetRoleByIdEndpoint
    : Cheetah.Modules.Identity.Api.Endpoints.GetRoleByIdEndpoint<GetRoleByIdRequest, GetRoleByIdQuery>
{
}
