using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Frontend.Navigation;
using __Prefix__.ModuleName.ApiClient;
using __Prefix__.ModuleName.Contracts;

namespace __Prefix__.ModuleName.Frontend;

[DependsOn(typeof(Cheetah.Blazor.Components.CrmBlazorComponentsModule))]
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(__ClassPrefix__ModuleNameApiClientModule))]
[DependsOn(typeof(__ClassPrefix__ModuleNameContractsModule))]
[DependsOn(typeof(CrmFrontendNavigationModule))]
public partial class __ClassPrefix__ModuleNameFrontendModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
