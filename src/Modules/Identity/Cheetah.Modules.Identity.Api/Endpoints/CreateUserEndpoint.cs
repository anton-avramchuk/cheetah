using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.Contracts.Requests;

namespace Cheetah.Modules.Identity.Api.Endpoints;

public abstract class CreateUserEndpoint<TRequest, TCommand> : CreateCommandEndpoint<TRequest, TCommand>
    where TRequest : CreateUserRequest
    where TCommand : CreateUserCommand
{
    public override string Route => Constants.UsersRoute;
    public override string GetByIdRouteName => "GetUserById";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("Users");
    }
}
