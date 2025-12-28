using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Tenants.Application.Queries;
using Cheetah.Tenants.Domain.Entities;

namespace Cheetah.Tenants.Application.Services;

[Export(LifetimeType.Scoped)]
public class TenantStore(IDispatcher dispatcher) : ITenantStore
{
    public async Task<Tenant?> FindByIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await dispatcher.QueryAsync<GetTenantByIdQuery, Tenant?>(new GetTenantByIdQuery(tenantId), cancellationToken);
    }

    public async Task<Tenant?> FindBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default)
    {
        return await dispatcher.QueryAsync<GetTenantBySubdomainQuery, Tenant?>(new GetTenantBySubdomainQuery(subdomain), cancellationToken);
    }

    public async Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dispatcher.QueryAsync<GetAllTenantsQuery, IReadOnlyList<Tenant>>(new GetAllTenantsQuery(), cancellationToken);
    }
}
