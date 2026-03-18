using Cheetah.Contracts.Attributes;

namespace Crm.Identity.Contracts.Requests;

public record DeleteUserRequest([FromRoute] Guid Id)
    : Cheetah.Modules.Identity.Contracts.Requests.DeleteUserRequest(Id);
