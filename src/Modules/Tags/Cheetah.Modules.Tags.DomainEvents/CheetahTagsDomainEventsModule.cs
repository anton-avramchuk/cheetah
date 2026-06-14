using Cheetah.Core.Events;
using Cheetah.Core.Modularity;

namespace Cheetah.Modules.Tags.DomainEvents;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmEventsCoreModule))]
public class CheetahTagsDomainEventsModule : CrmModule
{
}
