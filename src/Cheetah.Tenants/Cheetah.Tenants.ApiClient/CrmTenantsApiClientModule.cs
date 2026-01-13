using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Tenants.ApiClient.Implementation;
using Cheetah.Tenants.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Tenants.ApiClient;

/// <summary>
/// API Client module for Tenants.
/// Used by Blazor WASM frontend to communicate with backend API.
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmTenantsContractsModule))]
public partial class CrmTenantsApiClientModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Register HttpClient for TenantApiClient
        context.Services.AddHttpClient<TenantApiClient>(client =>
        {
            // Base address will be configured in the host application
            // For Blazor WASM, it's typically the backend API URL
        });

        // Services are registered via [Export] attribute by Source Generator
        RegisterServices(context.Services);
    }
}
