using Cheetah.Contracts.Attributes;

namespace Crm.Identity.Contracts.Requests;

public record GetUserByIdRequest([FromRoute] Guid Id)
    : Cheetah.Modules.Identity.Contracts.Requests.GetUserByIdRequest(Id);
