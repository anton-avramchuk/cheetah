using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Core.Tenants;

namespace Cheetah.Core.EntityFramework.Tenants;

[DependsOn(typeof(CrmEntityFrameworkModule))]
[DependsOn(typeof(CrmEventsCoreModule))]
[DependsOn(typeof(CrmTenantsCoreModule))]
public class CrmEntityFrameworkTenantsModule : CrmModule
{
}