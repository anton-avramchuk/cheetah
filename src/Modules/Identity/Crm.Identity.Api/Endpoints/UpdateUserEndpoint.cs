using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.Contracts.Requests;

namespace Crm.Identity.Api.Endpoints;

public sealed class UpdateUserEndpoint
    : Cheetah.Modules.Identity.Api.Endpoints.UpdateUserEndpoint<UpdateUserRequest, UpdateUserCommand>
{
}
