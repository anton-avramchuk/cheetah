using Cheetah.Modules.Identity.Application.Commands;
using Crm.Identity.Contracts.Requests;

namespace Crm.Identity.Api.Endpoints;

public sealed class CreateRoleEndpoint
    : Cheetah.Modules.Identity.Api.Endpoints.CreateRoleEndpoint<CreateRoleRequest, CreateRoleCommand>
{
}
