using AppName.Identity.Contracts.Requests;
using Cheetah.Modules.Identity.Application.Commands;

namespace AppName.Identity.Api.Endpoints;

public sealed class CreateUserEndpoint
    : Cheetah.Modules.Identity.Api.Endpoints.CreateUserEndpoint<CreateUserRequest, CreateUserCommand>
{
}
