using Cheetah.Core.CQRS;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Tenants.Domain;

namespace Cheetah.Tenants.Application;

[DependsOn(typeof(CrmTenantsDomainModule))]
[DependsOn(typeof(CrmCQRSCoreModule))]
[DependsOn(typeof(CrmEventsCoreModule))]
public partial class CrmTenantsApplicationModule : CrmModule
{
}
