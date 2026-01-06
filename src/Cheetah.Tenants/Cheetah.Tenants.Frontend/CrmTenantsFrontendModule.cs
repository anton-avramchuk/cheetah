using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Frontend.CQRS;
using Cheetah.Frontend.Events;
using Cheetah.Blazor.Components;
using Cheetah.Tenants.ApiClient;
using Cheetah.Tenants.Contracts;

namespace Cheetah.Tenants.Frontend;

/// <summary>
/// Blazor WASM Frontend module for Tenants.
/// Provides UI components and pages for tenant management.
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmFrontendCQRSModule))]
[DependsOn(typeof(CrmFrontendEventsModule))]
[DependsOn(typeof(CrmBlazorComponentsModule))]
[DependsOn(typeof(CrmTenantsApiClientModule))]
[DependsOn(typeof(CrmTenantsContractsModule))]
public partial class CrmTenantsFrontendModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Services are registered via [Export] attribute by Source Generator
        RegisterServices(context.Services);
    }
}
