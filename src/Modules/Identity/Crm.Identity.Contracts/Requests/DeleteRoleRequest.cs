using Cheetah.Contracts.Attributes;

namespace Crm.Identity.Contracts.Requests;

public record DeleteRoleRequest([FromRoute] Guid Id)
    : Cheetah.Modules.Identity.Contracts.Requests.DeleteRoleRequest(Id);
