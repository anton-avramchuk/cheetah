using AppName.Identity.Contracts.Requests;
using Cheetah.Modules.Identity.Application.Commands;

namespace AppName.Identity.Api.Endpoints;

public sealed class DeleteUserEndpoint
    : Cheetah.Modules.Identity.Api.Endpoints.DeleteUserEndpoint<DeleteUserRequest, DeleteUserCommand>
{
}
