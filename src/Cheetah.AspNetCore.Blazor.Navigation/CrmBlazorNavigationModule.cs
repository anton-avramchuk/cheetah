using Cheetah.Core;
using Cheetah.Core.Modularity;

namespace Cheetah.AspNetCore.Blazor.Navigation;

[DependsOn(typeof(CoreModule))]
public partial class CrmBlazorNavigationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
