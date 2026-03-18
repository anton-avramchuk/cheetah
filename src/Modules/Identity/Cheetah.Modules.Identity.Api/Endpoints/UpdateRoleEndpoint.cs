using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.Contracts.Requests;

namespace Cheetah.Modules.Identity.Api.Endpoints;

public abstract class UpdateRoleEndpoint<TRequest, TCommand> : UpdateCommandEndpoint<TRequest, TCommand>
    where TRequest : UpdateRoleRequest
    where TCommand : UpdateRoleCommand
{
    public override string Route => $"{Constants.RolesRoute}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("Roles");
    }
}
