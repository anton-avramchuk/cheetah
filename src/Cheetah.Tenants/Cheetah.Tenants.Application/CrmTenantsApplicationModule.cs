using Cheetah.Core.CQRS;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Tenants.DataAccess;
using Cheetah.Tenants.Domain;


namespace Cheetah.Tenants.Application;

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
