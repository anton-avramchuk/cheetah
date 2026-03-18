using Cheetah.Modules.Identity.Application.Commands;
using Crm.Identity.Contracts.Requests;

namespace Crm.Identity.Api.Endpoints;

public sealed class DeleteRoleEndpoint
    : Cheetah.Modules.Identity.Api.Endpoints.DeleteRoleEndpoint<DeleteRoleRequest, DeleteRoleCommand>
{
}
