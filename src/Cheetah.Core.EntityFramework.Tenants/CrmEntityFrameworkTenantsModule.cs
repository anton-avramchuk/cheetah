using Cheetah.Core.Modularity;
using Cheetah.Core.Tenants;

namespace Cheetah.Core.EntityFramework.Tenants;

[DependsOn(typeof(CrmEntityFrameworkModule),typeof(CrmTenantsCoreModule))]
public class CrmEntityFrameworkTenantsModule:CrmModule
{
}