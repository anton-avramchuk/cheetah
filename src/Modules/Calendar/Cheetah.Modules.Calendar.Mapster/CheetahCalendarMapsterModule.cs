using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Mapping.Mapster;
using Cheetah.Modules.Calendar.Application;
using Cheetah.Modules.Calendar.Contracts;

namespace Cheetah.Modules.Calendar.Mapster;

[DependsOn(typeof(CoreModule), typeof(CrmMapsterModule), typeof(CheetahCalendarContractsModule),
    typeof(CheetahCalendarApplicationModule))]
public partial class CheetahCalendarMapsterModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}