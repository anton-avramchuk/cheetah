namespace Crm.Identity.Contracts.Requests;

public record CreateUserRequest(string UserName, string Email, string Password, IReadOnlyList<Guid>? RoleIds = null)
    : Cheetah.Modules.Identity.Contracts.Requests.CreateUserRequest(UserName, Email, Password, RoleIds);
