using Cheetah.Modules.Identity.Application.Queries;
using Cheetah.Modules.Identity.Contracts.Requests;

namespace AppName.Identity.Api.Endpoints;

public sealed class GetUserByIdEndpoint
    : Cheetah.Modules.Identity.Api.Endpoints.GetUserByIdEndpoint<GetUserByIdRequest, GetUserByIdQuery>
{
}
