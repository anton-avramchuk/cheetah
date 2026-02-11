using Cheetah.Contracts;
using Cheetah.Core.Modularity;

namespace Crm.Candidates.Contracts;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmContractsModule))]
public class CrmCandidatesContractsModule : CrmModule
{
}