using Cheetah.Contracts;
using Cheetah.Core;
using Cheetah.Core.Cache;
using Cheetah.Core.Modularity;

namespace Cheetah.Blazor.Components;

/// <summary>
/// Blazor Components module.
/// Provides reusable UI components for all Blazor WASM applications.
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmContractsModule))]
[DependsOn(typeof(CrmCacheCoreModule))]
public partial class CrmBlazorComponentsModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Components are registered automatically
        RegisterServices(context.Services);
    }
}
