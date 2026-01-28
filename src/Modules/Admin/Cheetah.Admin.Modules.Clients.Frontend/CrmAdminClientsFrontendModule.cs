using Cheetah.Admin.Modules.Clients.Api.Client;
using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Frontend.Navigation;

namespace Cheetah.Admin.Modules.Clients.Frontend;

[DependsOn(typeof(Cheetah.Blazor.Components.CrmBlazorComponentsModule))]
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmAdminClientsApiClientModule))]
[DependsOn(typeof(CrmFrontendNavigationModule))]
public partial class CrmAdminClientsFrontendModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}