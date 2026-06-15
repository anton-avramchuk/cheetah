using Cheetah.Contracts;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Deals.Shared;

namespace Cheetah.Modules.Deals.Contracts;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmContractsModule), typeof(CheetahDealsSharedModule))]
public class CheetahDealsContractsModule : CrmModule
{
}
