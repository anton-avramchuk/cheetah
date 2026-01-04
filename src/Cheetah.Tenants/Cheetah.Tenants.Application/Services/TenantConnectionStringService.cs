using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Tenants.Services;
using Microsoft.Extensions.Configuration;

namespace Cheetah.Tenants.Application.Services;

/// <summary>
/// Service for generating tenant connection strings for all modules
/// </summary>
[Export(LifetimeType.Scoped, typeof(ITenantConnectionStringService))]
public class TenantConnectionStringService : ITenantConnectionStringService
{
    private readonly IEnumerable<IModuleConnectionStringProvider> _providers;
    private readonly IConfiguration _configuration;

    public TenantConnectionStringService(
        IEnumerable<IModuleConnectionStringProvider> providers,
        IConfiguration _configuration)
    {
        _providers = providers;
        this._configuration = _configuration;
    }

    public Task<Dictionary<string, string>> GenerateAllConnectionStringsAsync(Guid tenantId, string tenantName)
    {
        var result = new Dictionary<string, string>();

        // Get base connection string from configuration
        var baseConnectionString = _configuration.GetConnectionString("TenantTemplate")
            ?? throw new InvalidOperationException("TenantTemplate connection string is not configured");

        // Generate connection string for each registered module
        foreach (var provider in _providers)
        {
            var connectionString = provider.GenerateConnectionString(tenantId, tenantName, baseConnectionString);
            result[provider.ModuleName] = connectionString;
        }

        return Task.FromResult(result);
    }
}
