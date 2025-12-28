using Cheetah.Core.EntityFramework;
using Cheetah.Core.Modularity;
using Cheetah.Tenants.Application;

namespace Cheetah.Tenants.DataAccess;

[DependsOn(typeof(CrmTenantsApplicationModule))]
[DependsOn(typeof(CrmEntityFrameworkModule))]
public partial class CrmTenantsDataAccessModule : CrmModule
{
}
