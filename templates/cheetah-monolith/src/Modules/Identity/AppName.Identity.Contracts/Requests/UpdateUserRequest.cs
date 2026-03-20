using Cheetah.Contracts.Attributes;

namespace AppName.Identity.Contracts.Requests;

public record UpdateUserRequest([FromRoute] Guid Id, string UserName, string Email, IReadOnlyList<Guid>? RoleIds = null)
    : Cheetah.Modules.Identity.Contracts.Requests.UpdateUserRequest(Id, UserName, Email, RoleIds);
