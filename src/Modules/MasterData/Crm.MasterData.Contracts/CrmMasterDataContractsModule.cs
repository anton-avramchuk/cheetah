using Cheetah.Contracts;
using Cheetah.Core.Modularity;

namespace Crm.MasterData.Contracts;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmContractsModule))]
public partial class CrmMasterDataContractsModule : CrmModule
{
}