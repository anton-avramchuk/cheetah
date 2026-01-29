namespace Cheetah.Admin.Modules.Clients.Application;

public record ClientModel(Guid Id, string Name, string? Description, TenantModel? Tenant);

public record TenantModel(Guid Id, string Name);
