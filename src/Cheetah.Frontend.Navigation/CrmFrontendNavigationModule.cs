using Cheetah.Core.Modularity;

namespace Cheetah.Frontend.Navigation;

[DependsOn(typeof(Cheetah.Core.CoreModule))]
public partial class CrmFrontendNavigationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}