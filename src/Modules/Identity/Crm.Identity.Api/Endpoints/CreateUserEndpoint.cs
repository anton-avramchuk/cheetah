using Cheetah.Backend.Endpoints.Configuration;
using Cheetah.Backend.Endpoints.Http;
using Crm.Identity.Application.Commands;
using Crm.Identity.Contracts.Requests;

namespace Crm.Identity.Api.Endpoints;

public class CreateUserEndpoint : CreateCommandEndpoint<CreateUserRequest, CreateUserCommand>
{
    public override string Route => Constants.UsersRoute;
    public override string GetByIdRouteName => "GetUserById";

    protected override void Configure(EndpointConfiguration config)
    {
        config.WithTags("Users");
    }
}
