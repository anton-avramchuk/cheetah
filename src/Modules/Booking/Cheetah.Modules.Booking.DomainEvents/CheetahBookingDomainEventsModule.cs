using Cheetah.Core.Events;
using Cheetah.Core.Modularity;

namespace Cheetah.Modules.Booking.DomainEvents;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmEventsCoreModule))]
public class CheetahBookingDomainEventsModule : CrmModule
{
}
