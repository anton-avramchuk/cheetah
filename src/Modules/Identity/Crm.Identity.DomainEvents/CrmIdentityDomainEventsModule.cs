using Cheetah.Core.Modularity;
using Cheetah.Modules.Identity.DomainEvents;

namespace Crm.Identity.DomainEvents;

[DependsOn(typeof(CheetahIdentityDomainEventsModule))]
[DependsOn(typeof(Cheetah.Core.CoreModule))]
[DependsOn(typeof(Cheetah.Core.Events.CrmEventsCoreModule))]
public class CrmIdentityDomainEventsModule : CrmModule
{
}
