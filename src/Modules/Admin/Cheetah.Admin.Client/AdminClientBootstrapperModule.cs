using Cheetah.Admin.Modules.Clients.Frontend;
using Cheetah.Blazor;
using Cheetah.Blazor.Layout;
using Cheetah.Blazor.Layout.Extensions;
using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Frontend.Navigation;
using Cheetah.Mapping.Mapster;

namespace Cheetah.Admin.Client;

[DependsOn(typeof(CoreModule))]
[Bootstrapper]
[DependsOn(typeof(CrmBlazorModule))]
[DependsOn(typeof(CrmBlazorLayoutModule))]
[DependsOn(typeof(CrmAdminClientsFrontendModule))]
[DependsOn(typeof(CrmFrontendNavigationModule))]
[DependsOn(typeof(CrmMapsterModule))]
public partial class AdminClientBootstrapperModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);

        context.Services.ConfigureCrmLayout(config =>
        {
            config.ApplicationName = "Cheetah Admin";
            config.HomeUrl = "/";
            config.SidebarCollapsedByDefault = false;
        });
    }
}
