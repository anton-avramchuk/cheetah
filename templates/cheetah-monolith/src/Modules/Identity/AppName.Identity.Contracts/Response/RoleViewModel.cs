namespace AppName.Identity.Contracts.Response;

public record RoleViewModel(Guid Id, string Name)
    : Cheetah.Modules.Identity.Contracts.Response.RoleViewModel(Id, Name);
