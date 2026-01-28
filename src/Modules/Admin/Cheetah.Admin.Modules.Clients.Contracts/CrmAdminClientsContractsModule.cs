using Cheetah.Contracts;
using Cheetah.Core;
using Cheetah.Core.Modularity;

namespace Cheetah.Admin.Modules.Clients.Contracts;

[DependsOn(typeof(CoreModule), typeof(CrmContractsModule))]
public partial class CrmAdminClientsContractsModule : CrmModule
{
}