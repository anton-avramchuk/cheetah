using Cheetah.Contracts.Attributes;

namespace AppName.Identity.Contracts.Requests;

public record DeleteUserRequest([FromRoute] Guid Id)
    : Cheetah.Modules.Identity.Contracts.Requests.DeleteUserRequest(Id);
