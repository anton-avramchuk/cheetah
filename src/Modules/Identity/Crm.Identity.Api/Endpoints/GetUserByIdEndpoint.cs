using Cheetah.Modules.Identity.Application.Queries;
using Crm.Identity.Contracts.Requests;

namespace Crm.Identity.Api.Endpoints;

public sealed class GetUserByIdEndpoint
    : Cheetah.Modules.Identity.Api.Endpoints.GetUserByIdEndpoint<GetUserByIdRequest, GetUserByIdQuery>
{
}
