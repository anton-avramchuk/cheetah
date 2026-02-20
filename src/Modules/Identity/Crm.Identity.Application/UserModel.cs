namespace Crm.Identity.Application;

public record UserModel(Guid Id, string UserName, string Email, bool EmailConfirmed);
