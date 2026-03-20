using AppName.Identity.Contracts.Requests;
using Cheetah.Modules.Identity.Application.Queries;

namespace AppName.Identity.Api.Endpoints;

public sealed class GetUserByIdEndpoint
    : Cheetah.Modules.Identity.Api.Endpoints.GetUserByIdEndpoint<GetUserByIdRequest, GetUserByIdQuery>
{
}
