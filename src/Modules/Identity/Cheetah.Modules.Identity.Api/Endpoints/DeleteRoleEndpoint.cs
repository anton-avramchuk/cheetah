using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.Contracts.Requests;

namespace Cheetah.Modules.Identity.Api.Endpoints;

public abstract class DeleteRoleEndpoint<TRequest, TCommand> : DeleteCommandEndpoint<TRequest, TCommand>
    where TRequest : DeleteRoleRequest
    where TCommand : DeleteRoleCommand
{
    public override string Route => $"{Constants.RolesRoute}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("Roles");
    }
}
