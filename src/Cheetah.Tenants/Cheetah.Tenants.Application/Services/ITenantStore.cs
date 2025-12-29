using Cheetah.Tenants.Domain.Entities;

namespace Cheetah.Tenants.Application.Services;

public interface ITenantStore
{
    Task<Tenant?> FindByIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<Tenant?> FindBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets connection string for a specific tenant
    /// </summary>
    Task<string?> GetConnectionStringAsync(Guid tenantId, string name = "Default", CancellationToken ct = default);

    /// <summary>
    /// Gets all active tenants for database migration
    /// </summary>
    Task<List<Tenant>> GetAllActiveTenantsAsync(CancellationToken ct = default);
}
