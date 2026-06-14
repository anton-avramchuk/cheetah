using Cheetah.Contracts;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Calendar.Shared;

namespace Cheetah.Modules.Calendar.Contracts;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmContractsModule), typeof(CheetahCalendarSharedModule))]
public class CheetahCalendarContractsModule : CrmModule
{
}
