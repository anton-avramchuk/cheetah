using Cheetah.Contracts.Attributes;

namespace Crm.Identity.Contracts.Requests;

public record UpdateRoleRequest([FromRoute] Guid Id, string Name)
    : Cheetah.Modules.Identity.Contracts.Requests.UpdateRoleRequest(Id, Name);
