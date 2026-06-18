using Cheetah.Contracts;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Booking.Shared;

namespace Cheetah.Modules.Booking.Contracts;

[DependsOn(typeof(Cheetah.Core.CoreModule), typeof(CrmContractsModule), typeof(CheetahBookingSharedModule))]
public class CheetahBookingContractsModule : CrmModule
{
}
