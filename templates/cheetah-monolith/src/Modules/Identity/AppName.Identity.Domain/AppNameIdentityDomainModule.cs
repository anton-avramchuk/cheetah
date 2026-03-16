using Cheetah.Core.Modularity;
using Cheetah.Modules.Identity.Domain;

namespace AppName.Identity.Domain;

[DependsOn(typeof(CheetahIdentityDomainModule))]
public partial class AppNameIdentityDomainModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
