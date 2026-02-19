using Cheetah.Contracts;
using Cheetah.Core.Modularity;

namespace Crm.Identity.Contracts;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmContractsModule))]
public partial class CrmIdentityContractsModule : CrmModule
{
}