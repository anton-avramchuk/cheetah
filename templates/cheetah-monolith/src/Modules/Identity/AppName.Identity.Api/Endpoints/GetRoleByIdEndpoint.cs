using AppName.Identity.Contracts.Requests;
using Cheetah.Modules.Identity.Application.Queries;

namespace AppName.Identity.Api.Endpoints;

public sealed class GetRoleByIdEndpoint
    : Cheetah.Modules.Identity.Api.Endpoints.GetRoleByIdEndpoint<GetRoleByIdRequest, GetRoleByIdQuery>
{
}
