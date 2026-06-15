using Cheetah.Contracts;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Leads.Shared;

namespace Cheetah.Modules.Leads.Contracts;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmContractsModule), typeof(CheetahLeadsSharedModule))]
public class CheetahLeadsContractsModule : CrmModule
{
}
