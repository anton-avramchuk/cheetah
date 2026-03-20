using AppName.Identity.Contracts.Requests;
using Cheetah.Modules.Identity.Application.Commands;

namespace AppName.Identity.Api.Endpoints;

public sealed class UpdateUserEndpoint
    : Cheetah.Modules.Identity.Api.Endpoints.UpdateUserEndpoint<UpdateUserRequest, UpdateUserCommand>
{
}
