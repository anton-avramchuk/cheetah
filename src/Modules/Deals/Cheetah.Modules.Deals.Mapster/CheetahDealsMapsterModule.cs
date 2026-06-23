using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Mapping.Mapster;
using Cheetah.Modules.Deals.Application;
using Cheetah.Modules.Deals.Contracts;

namespace Cheetah.Modules.Deals.Mapster;

[DependsOn(typeof(CoreModule),
    typeof(CrmMapsterModule),typeof(CheetahDealsContractsModule),typeof(CheetahDealsApplicationModule))]
public partial class CheetahDealsMapsterModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}