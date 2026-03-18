using Cheetah.Modules.Identity.Application.Queries;
using Crm.Identity.Contracts.Requests;

namespace Crm.Identity.Api.Endpoints;

public sealed class GetAllUsersEndpoint
    : Cheetah.Modules.Identity.Api.Endpoints.GetAllUsersEndpoint<GetAllUsersRequest, GetAllUsersQuery>
{
}
