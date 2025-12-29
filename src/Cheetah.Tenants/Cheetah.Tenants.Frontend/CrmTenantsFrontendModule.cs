using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Frontend.CQRS;
using Cheetah.Frontend.Events;
using Cheetah.Tenants.ApiClient;

namespace Cheetah.Tenants.Frontend;

/// <summary>
/// Blazor WASM Frontend module for Tenants.
/// Provides UI components and pages for tenant management.
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmFrontendCQRSModule))]
[DependsOn(typeof(CrmFrontendEventsModule))]
[DependsOn(typeof(CrmTenantsApiClientModule))]
public partial class CrmTenantsFrontendModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Services are registered via [Export] attribute by Source Generator
        RegisterServices(context.Services);
    }
}
