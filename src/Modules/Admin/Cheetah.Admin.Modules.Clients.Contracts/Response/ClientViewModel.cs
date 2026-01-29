using Cheetah.Contracts.Responses;

namespace Cheetah.Admin.Modules.Clients.Contracts.Response;

public record ClientViewModel(Guid Id, string Name, string? Description, TenantViewModel? Tenant) : ICrmResponse;

public record TenantViewModel(Guid Id,string Name);