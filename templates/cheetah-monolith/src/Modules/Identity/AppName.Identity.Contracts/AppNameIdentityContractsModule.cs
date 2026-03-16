using Cheetah.Core.Modularity;
using Cheetah.Modules.Identity.Contracts;

namespace AppName.Identity.Contracts;

[DependsOn(typeof(CheetahIdentityContractsModule))]
public partial class AppNameIdentityContractsModule : CrmModule
{
}
