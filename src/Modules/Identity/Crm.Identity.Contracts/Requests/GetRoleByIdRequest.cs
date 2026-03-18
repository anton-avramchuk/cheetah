using Cheetah.Contracts.Attributes;

namespace Crm.Identity.Contracts.Requests;

public record GetRoleByIdRequest([FromRoute] Guid Id)
    : Cheetah.Modules.Identity.Contracts.Requests.GetRoleByIdRequest(Id);
