using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.Contracts.Requests;

namespace Crm.Identity.Api.Endpoints;

public sealed class DeleteUserEndpoint
    : Cheetah.Modules.Identity.Api.Endpoints.DeleteUserEndpoint<DeleteUserRequest, DeleteUserCommand>
{
}
