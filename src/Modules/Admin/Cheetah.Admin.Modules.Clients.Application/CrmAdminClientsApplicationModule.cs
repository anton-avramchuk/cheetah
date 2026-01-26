using Cheetah.Admin.Modules.Clients.Contracts;
using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;

namespace Cheetah.Admin.Modules.Clients.Application;

[DependsOn(typeof(Cheetah.Core.CoreModule),typeof(CrmAdminClientsContractsModule))]
[DependsOn(typeof(CrmCQRSCoreModule))]
public partial class CrmAdminClientsApplicationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}