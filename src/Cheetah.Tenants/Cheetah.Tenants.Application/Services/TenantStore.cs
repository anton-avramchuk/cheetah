using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Tenants.Application.Queries;
using Cheetah.Tenants.Domain.Entities;

namespace Cheetah.Tenants.Application.Services;

[Export(LifetimeType.Scoped, typeof(ITenantStore))]
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

    public async Task<string?> GetConnectionStringAsync(Guid tenantId, string name = "Default", CancellationToken ct = default)
    {
        var tenant = await FindByIdAsync(tenantId, ct);
        if (tenant == null)
            return null;

        var connectionString = tenant.ConnectionStrings.FirstOrDefault(cs => cs.Name == name);
        return connectionString?.ConnectionString;
    }

    public async Task<List<Tenant>> GetAllActiveTenantsAsync(CancellationToken ct = default)
    {
        var tenants = await GetAllAsync(ct);
        return tenants.Where(t => t.IsActive).ToList();
    }
}
