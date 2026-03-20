using AppName.Identity.Contracts.Requests;
using Cheetah.Modules.Identity.Application.Commands;

namespace AppName.Identity.Api.Endpoints;

public sealed class UpdateRoleEndpoint
    : Cheetah.Modules.Identity.Api.Endpoints.UpdateRoleEndpoint<UpdateRoleRequest, UpdateRoleCommand>
{
}
