using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Tenants.DataAccess;
using Cheetah.Tenants.Domain;
using Cheetah.Tenants.Events;

namespace Cheetah.Tenants.Application;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmDataAccessModule))]
[DependsOn(typeof(CrmTenantsEventsModule))]
[DependsOn(typeof(CrmTenantsDomainModule))]
[DependsOn(typeof(CrmCQRSCoreModule))]
[DependsOn(typeof(CrmEventsCoreModule))]
[DependsOn(typeof(CrmTenantsDataAccessModule))]
public partial class CrmTenantsApplicationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
