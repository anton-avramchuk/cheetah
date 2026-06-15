using Cheetah.Contracts;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Activities.Shared;

namespace Cheetah.Modules.Activities.Contracts;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmContractsModule), typeof(CheetahActivitiesSharedModule))]
public class CheetahActivitiesContractsModule : CrmModule
{
}
