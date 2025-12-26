using Cheetah.Core.Domain;
using Cheetah.Core.Events;
using Cheetah.Core.Modules;

namespace Cheetah.Core.Tenants;

[DependsOn(typeof(CrmDomainModule), typeof(CrmEventsCoreModule))]
public class CrmTenantsCoreModule : CrmModule
{
}

