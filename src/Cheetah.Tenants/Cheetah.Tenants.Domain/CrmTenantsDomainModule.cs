using Cheetah.Core;
using Cheetah.Core.Domain;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Core.Tenants;

namespace Cheetah.Tenants.Domain;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmDomainModule))]
[DependsOn(typeof(CrmEventsCoreModule))]
[DependsOn(typeof(CrmTenantsCoreModule))]
public partial class CrmTenantsDomainModule : CrmModule
{
}
