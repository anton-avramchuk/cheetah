using Cheetah.Core.Domain;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;

namespace Cheetah.Core.Tenants;

[DependsOn(typeof(CrmDomainModule), typeof(CrmEventsCoreModule))]
public class CrmTenantsCoreModule : CrmModule
{
}

