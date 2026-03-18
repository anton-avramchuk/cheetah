namespace Crm.Identity.Contracts.Response;

public record UserGridViewModel(Guid Id, string UserName, string Email)
    : Cheetah.Modules.Identity.Contracts.Response.UserGridViewModel(Id, UserName, Email);
