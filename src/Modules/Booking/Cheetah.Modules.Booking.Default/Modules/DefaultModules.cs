using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Extensions;
using Cheetah.Core;
using Cheetah.Core.Events;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Booking.Application;
using Cheetah.Modules.Booking.Application.Bookings;
using Cheetah.Modules.Booking.Application.Extensions;
using Cheetah.Modules.Booking.DomainEvents;
using Microsoft.Extensions.DependencyInjection;
using Cheetah.Modules.Booking.Contracts;
using Cheetah.Modules.Booking.Domain;
using Cheetah.Modules.Booking.Default.Contracts;
using Cheetah.Modules.Booking.Default.Endpoints;
using Cheetah.Modules.Booking.Default.Entities;
using Cheetah.Modules.Booking.Default.Factories;
using Cheetah.Modules.Booking.Default.Persistence;
using Cheetah.Modules.Booking.Infrastructure;
using Cheetah.Modules.Booking.Infrastructure.Extensions;
using BookingEntity = Cheetah.Modules.Booking.Default.Entities.Booking;

namespace Cheetah.Modules.Booking.Default.Modules;

/// <summary>
/// Модуль «из коробки»: закрывает шаблон Booking конкретными типами. В одной сборке должен быть ровно
/// один модуль (требование генератора), поэтому инфраструктура, прикладной слой и маппинг эндпоинтов
/// собраны здесь: <c>ConfigureServices</c> регистрирует DbContext/репозитории/шлюз Calendar
/// (<c>AddBookingInfrastructure</c>) и фабрики/проекторы/CQRS-handler'ы (<c>AddBookingApplication</c>),
/// а <c>OnApplicationInitialization</c> маппит конкретные эндпоинты.
/// </summary>
[DependsOn(typeof(CheetahBookingDomainModule),
    typeof(CheetahBookingInfrastructureModule),
    typeof(CheetahBookingApplicationModule),
    typeof(CheetahBookingContractsModule),
    typeof(CrmAspNetCoreModule))]
public partial class CheetahBookingDefaultModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var services = context.Services;

        services.AddBookingInfrastructure<BookingDbContext, BookingEntity, BookingType, AvailabilitySchedule>();

        services.AddBookingApplication<
            BookingType, BookingEntity, AvailabilitySchedule,
            CreateBookingTypeRequest, UpdateBookingTypeRequest, BookingTypeDto, BookingDto,
            BookingTypeFactory, BookingTypeProjector, BookingFactory, BookingProjector, ScheduleFactory>();
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        new BookingEndpoints().Map(context.GetRouteBuilder());

        // Синхронизация события Calendar при отмене/переносе брони.
        var eventBus = context.ServiceProvider.GetRequiredService<IEventBus>();
        eventBus.Subscribe<BookingCancelledIntegrationEvent, BookingCancelledCalendarSyncHandler<BookingEntity>>();
        eventBus.Subscribe<BookingRescheduledIntegrationEvent, BookingRescheduledCalendarSyncHandler<BookingEntity>>();
    }
}
