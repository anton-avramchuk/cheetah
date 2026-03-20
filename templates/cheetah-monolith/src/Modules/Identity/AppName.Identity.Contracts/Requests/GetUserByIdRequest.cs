using Cheetah.Contracts.Attributes;

namespace AppName.Identity.Contracts.Requests;

public record GetUserByIdRequest([FromRoute] Guid Id)
    : Cheetah.Modules.Identity.Contracts.Requests.GetUserByIdRequest(Id);
