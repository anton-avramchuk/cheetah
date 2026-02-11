using Cheetah.Contracts.Attributes;
using Cheetah.Core.Modularity;
using Crm.Candidates.Contracts;

namespace Crm.Candidates.ApiClient;

[GenerateApiClient("Candidates")]
[DependsOn(typeof(Cheetah.Core.CoreModule),
    typeof(CrmCandidatesContractsModule))]
public partial class CrmCandidatesApiClientModule : CrmModule
{
}