using Cheetah.Modules.Identity.Application.Commands;
using Crm.Identity.Contracts.Requests;

namespace Crm.Identity.Api.Endpoints;

public sealed class CreateUserEndpoint
    : Cheetah.Modules.Identity.Api.Endpoints.CreateUserEndpoint<CreateUserRequest, CreateUserCommand>
{
}
