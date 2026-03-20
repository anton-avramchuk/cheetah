using Cheetah.Contracts.Attributes;

namespace AppName.Identity.Contracts.Requests;

public record GetRoleByIdRequest([FromRoute] Guid Id)
    : Cheetah.Modules.Identity.Contracts.Requests.GetRoleByIdRequest(Id);
