namespace Cheetah.Admin.Modules.Clients.Application.Queries;

public record ClientModel(Guid Id, string Name, TenantModel Tenant);

public record TenantModel(Guid Id, string Name);
