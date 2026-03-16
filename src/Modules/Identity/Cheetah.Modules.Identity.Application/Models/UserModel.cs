namespace Cheetah.Modules.Identity.Application.Models;

public record UserModel(Guid Id, string UserName, string Email, bool EmailConfirmed);
