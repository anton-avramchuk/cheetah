using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Identity.Api;

namespace Crm.Identity.Api;

[DependsOn(typeof(CoreModule), typeof(CheetahIdentityApiModule))]
[Bootstrapper]
public partial class CrmIdentityBootstrapperModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
