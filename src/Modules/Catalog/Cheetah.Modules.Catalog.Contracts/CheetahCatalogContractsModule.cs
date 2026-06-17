using Cheetah.Contracts;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Catalog.Shared;

namespace Cheetah.Modules.Catalog.Contracts;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmContractsModule), typeof(CheetahCatalogSharedModule))]
public class CheetahCatalogContractsModule : CrmModule
{
}
