using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Frontend.Navigation;
using Crm.Identity.ApiClient;
using Crm.Identity.Contracts;

namespace Crm.Identity.Frontend;

[DependsOn(typeof(Cheetah.Blazor.Components.CrmBlazorComponentsModule))]
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmIdentityApiClientModule))]
[DependsOn(typeof(CrmIdentityContractsModule))]
[DependsOn(typeof(CrmFrontendNavigationModule))]
public partial class CrmIdentityFrontendModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}