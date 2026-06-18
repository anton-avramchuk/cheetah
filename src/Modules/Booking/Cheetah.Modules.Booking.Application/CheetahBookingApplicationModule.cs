using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Core.StateMachine;
using Cheetah.DistributedLock;
using Cheetah.Modules.Booking.Contracts;
using Cheetah.Modules.Booking.Domain;
using Cheetah.Modules.Booking.DomainEvents;
using Cheetah.Modules.Booking.Shared;

namespace Cheetah.Modules.Booking.Application;

/// <summary>
/// Прикладной слой шаблонного модуля Booking: generic CQRS (типы встреч, доступность, слоты, брони) +
/// конечный автомат статуса брони. Закрытые generic-handler'ы регистрирует наследник через
/// <c>AddBookingApplication&lt;…&gt;()</c>; шлюз к Calendar — Infrastructure.
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmDataAccessModule),
    typeof(CrmEventsCoreModule),
    typeof(CrmStateMachineModule),
    typeof(CrmDistributedLockModule),
    typeof(CheetahBookingDomainModule),
    typeof(CheetahBookingContractsModule),
    typeof(CheetahBookingDomainEventsModule))]
public partial class CheetahBookingApplicationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;

        // Конечный автомат статуса брони (терминальные: Cancelled/NoShow/Completed).
        services.AddStateMachine<BookingStatus>(sm => sm
            .From(BookingStatus.Confirmed).To(BookingStatus.Rescheduled, BookingStatus.Cancelled,
                BookingStatus.NoShow, BookingStatus.Completed)
            .From(BookingStatus.Rescheduled).To(BookingStatus.Rescheduled, BookingStatus.Cancelled,
                BookingStatus.NoShow, BookingStatus.Completed));

        RegisterServices(services);
    }
}
