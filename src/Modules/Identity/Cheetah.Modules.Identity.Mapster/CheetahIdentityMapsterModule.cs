using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Mapping.Mapster;
using Cheetah.Modules.Identity.Application;
using Cheetah.Modules.Identity.Contracts;

namespace Cheetah.Modules.Identity.Mapster;

[DependsOn(typeof(CoreModule),
    typeof(CrmMapsterModule), typeof(CheetahIdentityApplicationModule), typeof(CheetahIdentityContractsModule))]
public partial class CheetahIdentityMapsterModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}