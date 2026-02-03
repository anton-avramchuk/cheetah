using Cheetah.Blazor.Components;
using Cheetah.Blazor.Layout.Abstractions;
using Cheetah.Blazor.Layout.Configuration;
using Cheetah.Core.Modularity;
using Cheetah.Frontend.Navigation;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Blazor.Layout;

[DependsOn(
    typeof(Cheetah.Core.CoreModule),
    typeof(CrmBlazorComponentsModule),
    typeof(CrmFrontendNavigationModule)
)]
public partial class CrmBlazorLayoutModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);

        // Register default layout configuration if not already registered
        context.Services.AddSingleton<ILayoutConfiguration>(sp =>
            sp.GetService<LayoutConfiguration>() ?? new LayoutConfiguration());
    }
}
