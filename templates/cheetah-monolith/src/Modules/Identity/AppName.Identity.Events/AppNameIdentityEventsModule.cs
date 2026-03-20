using Cheetah.Core.Modularity;
using Cheetah.Modules.Identity.DomainEvents;

namespace AppName.Identity.Events;

[DependsOn(typeof(CheetahIdentityDomainEventsModule))]
[DependsOn(typeof(Cheetah.Core.CoreModule))]
[DependsOn(typeof(Cheetah.Core.Events.CrmEventsCoreModule))]
public partial class AppNameIdentityEventsModule : CrmModule
{
}
