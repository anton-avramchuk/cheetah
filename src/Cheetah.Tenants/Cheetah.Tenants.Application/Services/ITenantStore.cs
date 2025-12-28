using Cheetah.Tenants.Domain.Entities;

namespace Cheetah.Tenants.Application.Services;

public interface ITenantStore
{
    Task<Tenant?> FindByIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<Tenant?> FindBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken cancellationToken = default);
}
