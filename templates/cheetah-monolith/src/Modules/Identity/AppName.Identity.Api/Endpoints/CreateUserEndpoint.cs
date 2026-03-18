using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.Contracts.Requests;

namespace AppName.Identity.Api.Endpoints;

public sealed class CreateUserEndpoint
    : Cheetah.Modules.Identity.Api.Endpoints.CreateUserEndpoint<CreateUserRequest, CreateUserCommand>
{
}
