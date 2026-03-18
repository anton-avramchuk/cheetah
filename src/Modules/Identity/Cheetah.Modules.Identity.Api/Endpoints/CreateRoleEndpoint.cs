using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.Contracts.Requests;

namespace Cheetah.Modules.Identity.Api.Endpoints;

public abstract class CreateRoleEndpoint : CreateCommandEndpoint<CreateRoleRequest, CreateRoleCommand>
{
    public override string Route => Constants.RolesRoute;
    public override string GetByIdRouteName => "GetRoleById";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("Roles");
    }
}
