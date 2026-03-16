using Cheetah.Core.Modularity;
using Cheetah.Modules.Identity.Application;

namespace Crm.Identity.Application;

[DependsOn(typeof(CheetahIdentityApplicationModule))]
public partial class CrmIdentityApplicationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
