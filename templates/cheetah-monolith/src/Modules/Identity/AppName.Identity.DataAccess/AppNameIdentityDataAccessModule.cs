using Cheetah.Core.Modularity;
using Cheetah.Modules.Identity.DataAccess;

namespace AppName.Identity.DataAccess;

[DependsOn(typeof(CheetahIdentityDataAccessModule))]
public partial class AppNameIdentityDataAccessModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
