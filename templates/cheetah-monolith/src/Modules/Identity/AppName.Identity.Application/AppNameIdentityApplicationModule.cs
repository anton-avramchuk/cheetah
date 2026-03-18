using AppName.Identity.Domain;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Identity.Application;

namespace AppName.Identity.Application;

[DependsOn(typeof(CheetahIdentityApplicationModule), typeof(AppNameIdentityDomainModule))]
public partial class AppNameIdentityApplicationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
