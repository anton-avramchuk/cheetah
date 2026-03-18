namespace Crm.Identity.Contracts.Requests;

public record CreateRoleRequest(string Name)
    : Cheetah.Modules.Identity.Contracts.Requests.CreateRoleRequest(Name);
