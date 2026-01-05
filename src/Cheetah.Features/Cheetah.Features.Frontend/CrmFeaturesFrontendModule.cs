using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Frontend.CQRS;
using Cheetah.Frontend.Events;
using Cheetah.Blazor.Components;
using Cheetah.Features.Frontend.Client;

namespace Cheetah.Features.Frontend;

/// <summary>
/// Blazor WASM Frontend module for Features.
/// Provides UI components and pages for feature management.
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmFrontendCQRSModule))]
[DependsOn(typeof(CrmFrontendEventsModule))]
[DependsOn(typeof(CrmBlazorComponentsModule))]
[DependsOn(typeof(CrmFeaturesFrontendClientModule))]
public partial class CrmFeaturesFrontendModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Services are registered via [Export] attribute by Source Generator
        RegisterServices(context.Services);
    }
}
