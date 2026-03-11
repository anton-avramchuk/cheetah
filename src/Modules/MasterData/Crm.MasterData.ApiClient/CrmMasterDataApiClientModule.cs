using Cheetah.Contracts.Attributes;
using Cheetah.Core.Modularity;
using Crm.MasterData.Contracts;

namespace Crm.MasterData.ApiClient;

[GenerateApiClient("MasterData")]
[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmMasterDataContractsModule))]
public partial class CrmMasterDataApiClientModule : CrmModule
{
}