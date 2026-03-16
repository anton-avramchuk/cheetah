using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Cheetah.Modules.Identity.Application.Models;
using Cheetah.Modules.Identity.Application.Queries;
using Cheetah.Modules.Identity.Contracts.Requests;
using Cheetah.Modules.Identity.Contracts.Response;

namespace Cheetah.Modules.Identity.Api.Endpoints;

public class GetRoleByIdEndpoint : QueryOrNotFoundEndpoint<GetRoleByIdRequest, GetRoleByIdQuery, RoleModel, RoleViewModel>
{
    public override string Route => $"{Constants.RolesRoute}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithName("GetRoleById").WithTags("Roles");
    }
}
