using Cheetah.AspNetCore;
using Cheetah.Core.Modularity;
using Cheetah.Tenants.Application;

namespace Cheetah.Tenants.Api;

[DependsOn(typeof(CrmTenantsApplicationModule))]
[DependsOn(typeof(CrmAspNetCoreModule))]
public partial class CrmTenantsApiModule : CrmModule
{
}
