using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.Contracts.Requests;

namespace Cheetah.Modules.Identity.Api.Endpoints;

public abstract class DeleteUserEndpoint<TRequest, TCommand> : DeleteCommandEndpoint<TRequest, TCommand>
    where TRequest : DeleteUserRequest
    where TCommand : DeleteUserCommand
{
    public override string Route => $"{Constants.UsersRoute}/{{id:guid}}";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("Users");
    }
}
