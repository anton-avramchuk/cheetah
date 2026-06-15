using Cheetah.Core.Events;
using Cheetah.Core.Modularity;

namespace Cheetah.Modules.Deals.DomainEvents;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmEventsCoreModule))]
public class CheetahDealsDomainEventsModule : CrmModule
{
}
