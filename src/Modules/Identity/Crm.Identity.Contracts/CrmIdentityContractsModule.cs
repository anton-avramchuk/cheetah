using Cheetah.Core.Modularity;
using Cheetah.Modules.Identity.Contracts;

namespace Crm.Identity.Contracts;

[DependsOn(typeof(CheetahIdentityContractsModule))]
public partial class CrmIdentityContractsModule : CrmModule
{
}
