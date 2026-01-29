using Cheetah.Admin.Modules.Clients.Frontend;
using Cheetah.Blazor;
using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Frontend.Navigation;

namespace Cheetah.Admin.Client;

[DependsOn(typeof(CoreModule))]
[Bootstrapper]
[DependsOn(typeof(CrmBlazorModule))]
[DependsOn(typeof(CrmAdminClientsFrontendModule))]
[DependsOn(typeof(CrmFrontendNavigationModule))]
public partial class AdminClientBootstrapperModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}