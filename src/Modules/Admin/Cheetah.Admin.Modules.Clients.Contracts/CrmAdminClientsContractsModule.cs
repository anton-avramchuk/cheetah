using Cheetah.AspNetCore.Contracts;
using Cheetah.Core;
using Cheetah.Core.Modularity;

namespace Cheetah.Admin.Modules.Clients.Contracts;

[DependsOn(typeof(CoreModule),typeof(CrmAspNetCoreContractsModule))]


public class CrmAdminClientsContractsModule:CrmModule
{
}