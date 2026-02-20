using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.Identity.Application.Commands;
using Crm.Identity.Contracts.Requests;

namespace Crm.Identity.Api.Endpoints;

public class CreateRoleEndpoint : CreateCommandEndpoint<CreateRoleRequest, CreateRoleCommand>
{
    public override string Route => Constants.RolesRoute;
    public override string GetByIdRouteName => "GetRoleById";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("Roles");
    }
}
