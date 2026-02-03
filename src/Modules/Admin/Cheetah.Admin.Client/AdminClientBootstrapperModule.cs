using Cheetah.Admin.Modules.Clients.Frontend;
using Cheetah.Blazor;
using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Frontend.Navigation;
using Cheetah.Mapping.Mapster;

namespace Cheetah.Admin.Client;

[DependsOn(typeof(CoreModule))]
[Bootstrapper]
[DependsOn(typeof(CrmBlazorModule))]
[DependsOn(typeof(CrmAdminClientsFrontendModule))]
[DependsOn(typeof(CrmFrontendNavigationModule))]
[DependsOn(typeof(CrmMapsterModule))]
public partial class AdminClientBootstrapperModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}