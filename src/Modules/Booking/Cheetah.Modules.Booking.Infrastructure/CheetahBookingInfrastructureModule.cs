using Cheetah.Core;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Domain;
using Cheetah.Core.EntityFramework;
using Cheetah.Core.EntityFramework.PostgreSql;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Booking.Application;
using Cheetah.Modules.Booking.Domain;
using Cheetah.Modules.Calendar.Client;

namespace Cheetah.Modules.Booking.Infrastructure;

/// <summary>
/// Инфраструктура шаблонного модуля Booking: абстрактные базы EF
/// (<see cref="Persistence.BookingDbContextBase{TContext,TBooking,TBookingType,TSchedule}"/> и
/// конфигурации агрегатов), generic-регистрация через <c>AddBookingInfrastructure&lt;…&gt;()</c> и шлюз
/// к Calendar (<see cref="Calendar.BookingCalendarGateway"/>). Конкретный DbContext, конфигурации и
/// миграции создаёт наследник.
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmDataAccessModule),
    typeof(CrmDomainModule),
    typeof(CrmEntityFrameworkModule),
    typeof(CrmEntityFrameworkPostgreSqlModule),
    typeof(CheetahBookingDomainModule),
    typeof(CheetahBookingApplicationModule),
    typeof(CheetahCalendarClientModule))]
public partial class CheetahBookingInfrastructureModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
