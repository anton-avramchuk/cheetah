using AppName.Identity.Application;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Identity.Api;

namespace AppName.Identity.Api;

[DependsOn(typeof(CheetahIdentityApiModule), typeof(AppNameIdentityApplicationModule))]
public partial class AppNameIdentityApiModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
