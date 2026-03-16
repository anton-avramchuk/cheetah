using Cheetah.Core.Modularity;
using Cheetah.Modules.Identity.Application;

namespace AppName.Identity.Application;

[DependsOn(typeof(CheetahIdentityApplicationModule))]
public partial class AppNameIdentityApplicationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
