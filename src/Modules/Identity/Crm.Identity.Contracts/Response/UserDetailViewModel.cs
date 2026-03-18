namespace Crm.Identity.Contracts.Response;

public record UserDetailViewModel(Guid Id, string UserName, string Email, IReadOnlyList<Guid> RoleIds)
    : Cheetah.Modules.Identity.Contracts.Response.UserDetailViewModel(Id, UserName, Email, RoleIds);
