using Cheetah.Contracts;
using Cheetah.Core.Modularity;

namespace Cheetah.Modules.Identity.Contracts;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmContractsModule))]
public partial class CheetahIdentityContractsModule : CrmModule
{
}
