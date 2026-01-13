using Cheetah.Core;
using Cheetah.Core.Domain;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Core.Tenants;
using Cheetah.Tenants.Events;

namespace Cheetah.Tenants.Domain;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmDomainModule))]
[DependsOn(typeof(CrmEventsCoreModule))]
[DependsOn(typeof(CrmTenantsCoreModule))]
[DependsOn(typeof(CrmTenantsEventsModule))]
public partial class CrmTenantsDomainModule : CrmModule
{
}
