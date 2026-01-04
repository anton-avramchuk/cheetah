using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Tenants.Services;
using Cheetah.Tenants.Client.Interfaces;

namespace Cheetah.Tenants.Client.Services;

/// <summary>
/// Adapter for ITenantClientService to ITenantMigrationService
/// </summary>
[Export(LifetimeType.Scoped, typeof(ITenantMigrationService))]
public class TenantMigrationServiceAdapter : ITenantMigrationService
{
    private readonly ITenantClientService _tenantClient;

    public TenantMigrationServiceAdapter(ITenantClientService tenantClient)
    {
        _tenantClient = tenantClient;
    }

    public async ValueTask<List<TenantMigrationInfo>> GetAllActiveTenantsAsync(CancellationToken ct = default)
    {
        var tenants = await _tenantClient.GetAllActiveAsync(ct);
        return tenants.Select(t => new TenantMigrationInfo(t.Id, t.Name)).ToList();
    }

    public async ValueTask<string?> GetConnectionStringAsync(Guid tenantId, string moduleName, CancellationToken ct = default)
    {
        return await _tenantClient.GetConnectionStringAsync(tenantId, moduleName, ct);
    }
}
