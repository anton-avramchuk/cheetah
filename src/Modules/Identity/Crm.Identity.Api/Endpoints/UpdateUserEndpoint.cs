using Cheetah.Modules.Identity.Application.Commands;
using Crm.Identity.Contracts.Requests;

namespace Crm.Identity.Api.Endpoints;

public sealed class UpdateUserEndpoint
    : Cheetah.Modules.Identity.Api.Endpoints.UpdateUserEndpoint<UpdateUserRequest, UpdateUserCommand>
{
}
