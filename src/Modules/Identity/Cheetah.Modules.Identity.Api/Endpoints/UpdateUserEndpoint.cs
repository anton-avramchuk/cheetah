using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.Contracts.Requests;

namespace Cheetah.Modules.Identity.Api.Endpoints;

public abstract class UpdateUserEndpoint<TRequest, TCommand> : UpdateCommandEndpoint<TRequest, TCommand>
    where TRequest : UpdateUserRequest
    where TCommand : UpdateUserCommand
{
    public override string Route => $"{Constants.UsersRoute}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("Users");
    }
}
