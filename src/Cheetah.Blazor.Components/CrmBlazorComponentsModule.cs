using Cheetah.Core.Modularity;

namespace Cheetah.Blazor.Components;

/// <summary>
/// Blazor Components module.
/// Provides reusable UI components for all Blazor WASM applications.
/// </summary>
public partial class CrmBlazorComponentsModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Components are registered automatically
        RegisterServices(context.Services);
    }
}
