namespace Crm.Identity.Application;

public record UserDetailModel(Guid Id, string UserName, string Email, bool EmailConfirmed, IReadOnlyList<Guid> RoleIds);
