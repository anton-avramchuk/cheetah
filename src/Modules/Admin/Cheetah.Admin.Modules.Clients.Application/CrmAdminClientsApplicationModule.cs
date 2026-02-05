using Cheetah.Admin.Modules.Clients.Contracts;
using Cheetah.Admin.Modules.Clients.Domain;
using Cheetah.Core.CQRS;
using Cheetah.Core.Events;
using Cheetah.Core.Grid;
using Cheetah.Core.Modularity;

namespace Cheetah.Admin.Modules.Clients.Application;

[DependsOn(typeof(Cheetah.Core.CoreModule))]
[DependsOn(typeof(CrmAdminClientsContractsModule))]
[DependsOn(typeof(CrmCQRSCoreModule))]
[DependsOn(typeof(CrmEventsCoreModule))]
[DependsOn(typeof(CrmGridModule))]
[DependsOn(typeof(CrmAdminClientsDomainModule))]
public partial class CrmAdminClientsApplicationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
