using Cheetah.Core.Modularity;
using Crm.Candidates.Contracts;

namespace Crm.Candidates.ApiClient;

[DependsOn(typeof(Cheetah.Core.CoreModule),
    typeof(CrmCandidatesContractsModule))]
public partial class CrmCandidatesApiClientModule : CrmModule
{
}