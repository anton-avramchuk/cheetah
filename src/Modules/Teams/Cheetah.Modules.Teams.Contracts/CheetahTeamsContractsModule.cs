using Cheetah.Contracts;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Teams.Shared;

namespace Cheetah.Modules.Teams.Contracts;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmContractsModule), typeof(CheetahTeamsSharedModule))]
public class CheetahTeamsContractsModule : CrmModule
{
}
