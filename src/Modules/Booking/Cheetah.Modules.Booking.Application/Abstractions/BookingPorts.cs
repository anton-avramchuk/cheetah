using Cheetah.Modules.Booking.Contracts;
using Cheetah.Modules.Booking.Domain.Entities;
using Cheetah.Modules.Booking.Domain.Slots;

namespace Cheetah.Modules.Booking.Application.Abstractions;

/// <summary>
/// Фабрика конкретного типа встречи из запроса на создание. Реализуется наследником — он знает, как
/// сконструировать свою сущность (включая доп. поля) через <c>InitializeCore</c>.
/// </summary>
public interface IBookingTypeFactory<out TBookingType, in TCreateRequest>
    where TBookingType : BookingTypeBase
    where TCreateRequest : CreateBookingTypeRequestBase
{
    TBookingType Create(TCreateRequest request);
}

/// <summary>Проекция конкретного типа встречи в конкретный DTO (вкл. доп. поля наследника).</summary>
public interface IBookingTypeProjector<in TBookingType, out TDto>
    where TBookingType : BookingTypeBase
    where TDto : BookingTypeDtoBase
{
    TDto ToDto(TBookingType type);
}

/// <summary>Фабрика конкретного расписания доступности host'а.</summary>
public interface IScheduleFactory<out TSchedule>
    where TSchedule : AvailabilityScheduleBase
{
    TSchedule Create(Guid hostUserId, string timeZoneId);
}

/// <summary>
/// Фабрика конкретной брони из подтверждённого слота и публичного запроса. Реализуется наследником:
/// конструирует сущность через <c>InitializeCore</c> (генерация токена, событие подтверждения).
/// </summary>
public interface IBookingFactory<out TBooking, in TBookingType>
    where TBooking : BookingBase
    where TBookingType : BookingTypeBase
{
    TBooking Create(TBookingType type, DateTimeOffset startUtc, CreatePublicBookingRequest request);
}

/// <summary>Проекция конкретной брони в конкретный DTO (вкл. доп. поля наследника).</summary>
public interface IBookingProjector<in TBooking, out TDto>
    where TBooking : BookingBase
    where TDto : BookingDtoBase
{
    TDto ToDto(TBooking booking);
}

/// <summary>
/// Порт к Calendar (реализуется в Infrastructure поверх <c>Calendar.Client</c>): занятость host'а
/// (free/busy) и создание итогового события встречи. Так Application не зависит от Calendar.Client.
/// </summary>
public interface IBookingCalendarGateway
{
    /// <summary>Занятые интервалы host'а в окне <c>[fromUtc, toUtc)</c> (для вычитания из доступности).</summary>
    ValueTask<IReadOnlyList<BusyInterval>> GetBusyAsync(
        Guid hostUserId, DateTimeOffset fromUtc, DateTimeOffset toUtc, CancellationToken ct = default);

    /// <summary>Создать событие в календаре host'а для подтверждённой брони. Возвращает Id события (или null).</summary>
    ValueTask<Guid?> CreateEventAsync(BookingBase booking, BookingTypeBase type, CancellationToken ct = default);

    /// <summary>Перенести ранее созданное событие брони на новое время (при переносе брони).</summary>
    ValueTask RescheduleEventAsync(Guid calendarEventId, DateTimeOffset startUtc, DateTimeOffset endUtc, CancellationToken ct = default);

    /// <summary>Отменить ранее созданное событие брони (при отмене брони).</summary>
    ValueTask CancelEventAsync(Guid calendarEventId, CancellationToken ct = default);
}

/// <summary>
/// Точка расширения «что сделать на подтверждённой брони» (создать Lead/Activity/своё). Реализация
/// по умолчанию — <see cref="NullBookingConfirmationHandler{TBooking}"/> (ничего не делает).
/// </summary>
public interface IBookingConfirmationHandler<in TBooking>
    where TBooking : BookingBase
{
    ValueTask OnConfirmedAsync(TBooking booking, BookingTypeBase type, CancellationToken ct = default);
}

/// <summary>Обработчик подтверждения по умолчанию: no-op.</summary>
public sealed class NullBookingConfirmationHandler<TBooking> : IBookingConfirmationHandler<TBooking>
    where TBooking : BookingBase
{
    public ValueTask OnConfirmedAsync(TBooking booking, BookingTypeBase type, CancellationToken ct = default)
        => ValueTask.CompletedTask;
}
