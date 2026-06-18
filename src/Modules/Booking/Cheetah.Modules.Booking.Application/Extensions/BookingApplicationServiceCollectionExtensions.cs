using Cheetah.Core.CQRS;
using Cheetah.Modules.Booking.Application.Abstractions;
using Cheetah.Modules.Booking.Application.Availability;
using Cheetah.Modules.Booking.Application.Bookings;
using Cheetah.Modules.Booking.Application.BookingTypes;
using Cheetah.Modules.Booking.Application.Slots;
using Cheetah.Modules.Booking.Contracts;
using Cheetah.Modules.Booking.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cheetah.Modules.Booking.Application.Extensions;

public static class BookingApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует фабрики, проекторы и закрытые generic CQRS-handler'ы конкретной реализации Booking.
    /// Вызывается из прикладного модуля наследника после <c>AddBookingInfrastructure</c>. Шлюз к Calendar
    /// (<see cref="IBookingCalendarGateway"/>) регистрируется в Infrastructure; обработчик подтверждения
    /// по умолчанию — no-op (наследник может переопределить, зарегистрировав свой до вызова этого метода).
    /// </summary>
    public static IServiceCollection AddBookingApplication<
        TBookingType, TBooking, TSchedule,
        TCreateTypeRequest, TUpdateTypeRequest, TBookingTypeDto, TBookingDto,
        TBookingTypeFactory, TBookingTypeProjector, TBookingFactory, TBookingProjector, TScheduleFactory>(
        this IServiceCollection services)
        where TBookingType : BookingTypeBase
        where TBooking : BookingBase
        where TSchedule : AvailabilityScheduleBase
        where TCreateTypeRequest : CreateBookingTypeRequestBase
        where TUpdateTypeRequest : UpdateBookingTypeRequestBase
        where TBookingTypeDto : BookingTypeDtoBase
        where TBookingDto : BookingDtoBase
        where TBookingTypeFactory : class, IBookingTypeFactory<TBookingType, TCreateTypeRequest>
        where TBookingTypeProjector : class, IBookingTypeProjector<TBookingType, TBookingTypeDto>
        where TBookingFactory : class, IBookingFactory<TBooking, TBookingType>
        where TBookingProjector : class, IBookingProjector<TBooking, TBookingDto>
        where TScheduleFactory : class, IScheduleFactory<TSchedule>
    {
        // Фабрики/проекторы наследника.
        services.AddScoped<IBookingTypeFactory<TBookingType, TCreateTypeRequest>, TBookingTypeFactory>();
        services.AddScoped<IBookingTypeProjector<TBookingType, TBookingTypeDto>, TBookingTypeProjector>();
        services.AddScoped<IBookingFactory<TBooking, TBookingType>, TBookingFactory>();
        services.AddScoped<IBookingProjector<TBooking, TBookingDto>, TBookingProjector>();
        services.AddScoped<IScheduleFactory<TSchedule>, TScheduleFactory>();

        // Точка расширения «что сделать на подтверждённой брони» — по умолчанию no-op.
        services.TryAddScoped<IBookingConfirmationHandler<TBooking>, NullBookingConfirmationHandler<TBooking>>();

        // Типы встреч.
        services.AddScoped<ICommandHandler<CreateBookingTypeCommand<TCreateTypeRequest>, Guid>,
            CreateBookingTypeCommandHandler<TBookingType, TCreateTypeRequest>>();
        services.AddScoped<ICommandHandler<UpdateBookingTypeCommand<TUpdateTypeRequest>>,
            UpdateBookingTypeCommandHandler<TBookingType, TUpdateTypeRequest>>();
        services.AddScoped<ICommandHandler<DeactivateBookingTypeCommand>,
            DeactivateBookingTypeCommandHandler<TBookingType>>();
        services.AddScoped<IQueryHandler<GetPublicPageQuery, PublicBookingPageDto?>,
            GetPublicPageQueryHandler<TBookingType>>();
        services.AddScoped<IQueryHandler<GetBookingTypeByIdQuery<TBookingTypeDto>, TBookingTypeDto?>,
            GetBookingTypeByIdQueryHandler<TBookingType, TBookingTypeDto>>();

        // Доступность.
        services.AddScoped<ICommandHandler<UpsertAvailabilityCommand, Guid>,
            UpsertAvailabilityCommandHandler<TSchedule>>();

        // Слоты.
        services.AddScoped<IQueryHandler<GetAvailableSlotsQuery, IReadOnlyList<SlotDto>>,
            GetAvailableSlotsQueryHandler<TBookingType, TSchedule>>();

        // Брони.
        services.AddScoped<ICommandHandler<CreateBookingCommand, Guid>,
            CreateBookingCommandHandler<TBooking, TBookingType>>();
        services.AddScoped<ICommandHandler<RescheduleBookingCommand>,
            RescheduleBookingCommandHandler<TBooking>>();
        services.AddScoped<ICommandHandler<CancelBookingCommand>,
            CancelBookingCommandHandler<TBooking>>();
        services.AddScoped<ICommandHandler<MarkNoShowCommand>,
            MarkNoShowCommandHandler<TBooking>>();
        services.AddScoped<IQueryHandler<GetBookingByIdQuery<TBookingDto>, TBookingDto?>,
            GetBookingByIdQueryHandler<TBooking, TBookingDto>>();
        services.AddScoped<IQueryHandler<ListBookingsQuery<TBookingDto>, IReadOnlyList<TBookingDto>>,
            ListBookingsQueryHandler<TBooking, TBookingDto>>();

        // Подписчики синхронизации события Calendar (подписка — в OnApplicationInitialization наследника).
        services.AddScoped<BookingCancelledCalendarSyncHandler<TBooking>>();
        services.AddScoped<BookingRescheduledCalendarSyncHandler<TBooking>>();

        return services;
    }
}
