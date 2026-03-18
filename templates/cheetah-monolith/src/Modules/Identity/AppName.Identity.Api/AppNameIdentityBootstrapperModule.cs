using AppName.Identity.Application;
using AppName.Identity.DataAccess;
using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Identity.Api;

namespace AppName.Identity.Api;

[DependsOn(typeof(CoreModule), typeof(CheetahIdentityApiModule), typeof(AppNameIdentityApplicationModule), typeof(AppNameIdentityDataAccessModule))]
[Bootstrapper]
public partial class AppNameIdentityBootstrapperModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
