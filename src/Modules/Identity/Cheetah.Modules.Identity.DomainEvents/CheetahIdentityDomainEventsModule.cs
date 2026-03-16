using Cheetah.Core.Events;
using Cheetah.Core.Modularity;

namespace Cheetah.Modules.Identity.DomainEvents;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmEventsCoreModule))]
public class CheetahIdentityDomainEventsModule : CrmModule
{
}
