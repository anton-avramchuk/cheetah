using Cheetah.Contracts.Attributes;

namespace AppName.Identity.Contracts.Requests;

public record DeleteRoleRequest([FromRoute] Guid Id)
    : Cheetah.Modules.Identity.Contracts.Requests.DeleteRoleRequest(Id);
