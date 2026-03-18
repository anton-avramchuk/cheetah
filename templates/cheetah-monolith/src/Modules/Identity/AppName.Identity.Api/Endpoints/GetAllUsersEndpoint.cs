using Cheetah.Modules.Identity.Application.Queries;
using Cheetah.Modules.Identity.Contracts.Requests;

namespace AppName.Identity.Api.Endpoints;

public sealed class GetAllUsersEndpoint
    : Cheetah.Modules.Identity.Api.Endpoints.GetAllUsersEndpoint<GetAllUsersRequest, GetAllUsersQuery>
{
}
