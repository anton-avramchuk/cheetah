using Cheetah.Core;
using Cheetah.Core.Domain;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;

namespace Cheetah.Tenants.Domain;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmDomainModule))]
[DependsOn(typeof(CrmEventsCoreModule))]
public partial class CrmTenantsDomainModule : CrmModule
{
}
