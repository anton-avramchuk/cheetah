using Cheetah.Contracts;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Tags.Shared;

namespace Cheetah.Modules.Tags.Contracts;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmContractsModule), typeof(CheetahTagsSharedModule))]
public class CheetahTagsContractsModule : CrmModule
{
}
