using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.Identity.Application;
using Crm.Identity.Application.Queries;
using Crm.Identity.Contracts.Requests;
using Crm.Identity.Contracts.Response;

namespace Crm.Identity.Api.Endpoints;

public class GetRoleByIdEndpoint : QueryOrNotFoundEndpoint<GetRoleByIdRequest, GetRoleByIdQuery, RoleModel, RoleViewModel>
{
    public override string Route => $"{Constants.RolesRoute}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithName("GetRoleById").WithTags("Roles");
    }
}
