namespace Cheetah.Modules.Identity.Application.Models;

public record UserDetailModel(Guid Id, string UserName, string Email, bool EmailConfirmed, IReadOnlyList<Guid> RoleIds);
