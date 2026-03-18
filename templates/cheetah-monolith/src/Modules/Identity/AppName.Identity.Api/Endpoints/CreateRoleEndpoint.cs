using Cheetah.Modules.Identity.Application.Commands;
using Cheetah.Modules.Identity.Contracts.Requests;

namespace AppName.Identity.Api.Endpoints;

public sealed class CreateRoleEndpoint
    : Cheetah.Modules.Identity.Api.Endpoints.CreateRoleEndpoint<CreateRoleRequest, CreateRoleCommand>
{
}
