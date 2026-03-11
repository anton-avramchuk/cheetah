using Cheetah.Contracts;
using Cheetah.Core.Modularity;
using Crm.MasterData.Domain;

namespace Crm.MasterData.Contracts;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmContractsModule), typeof(CrmMasterDataDomainModule))]
public partial class CrmMasterDataContractsModule : CrmModule
{
}
