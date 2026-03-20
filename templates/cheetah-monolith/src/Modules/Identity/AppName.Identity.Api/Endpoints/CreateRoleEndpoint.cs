using AppName.Identity.Contracts.Requests;
using Cheetah.Modules.Identity.Application.Commands;

namespace AppName.Identity.Api.Endpoints;

public sealed class CreateRoleEndpoint
    : Cheetah.Modules.Identity.Api.Endpoints.CreateRoleEndpoint<CreateRoleRequest, CreateRoleCommand>
{
}
