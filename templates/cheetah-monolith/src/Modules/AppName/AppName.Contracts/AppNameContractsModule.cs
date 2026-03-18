using Cheetah.Contracts;
using Cheetah.Core.Modularity;

namespace AppName.Contracts;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmContractsModule))]
public partial class AppNameContractsModule : CrmModule
{
}
