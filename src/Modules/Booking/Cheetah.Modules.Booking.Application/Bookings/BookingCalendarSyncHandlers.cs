using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Modules.Booking.Application.Abstractions;
using Cheetah.Modules.Booking.Domain.Entities;
using Cheetah.Modules.Booking.DomainEvents;

namespace Cheetah.Modules.Booking.Application.Bookings;

/// <summary>
/// Синхронизирует событие Calendar при отмене брони: загружает бронь по Id и, если за ней закреплено
/// событие (<see cref="BookingBase.CalendarEventId"/>), отменяет его. Идемпотентно: нет события —
/// нет действия (отмена в Calendar тоже мягкая/идемпотентная). Generic по конкретному типу брони.
/// </summary>
public class BookingCancelledCalendarSyncHandler<TBooking> : IEventHandler<BookingCancelledIntegrationEvent>
    where TBooking : BookingBase
{
    private readonly IRepository<TBooking, Guid> _bookings;
    private readonly IBookingCalendarGateway _calendar;

    public BookingCancelledCalendarSyncHandler(IRepository<TBooking, Guid> bookings, IBookingCalendarGateway calendar)
    {
        _bookings = bookings;
        _calendar = calendar;
    }

    public async ValueTask HandleAsync(BookingCancelledIntegrationEvent @event, CancellationToken ct = default)
    {
        var booking = await _bookings.GetByIdAsync(@event.BookingId, ct);
        if (booking?.CalendarEventId is { } eventId)
            await _calendar.CancelEventAsync(eventId, ct);
    }
}

/// <summary>
/// Синхронизирует событие Calendar при переносе брони: переносит закреплённое событие на новое время
/// из события. Generic по конкретному типу брони.
/// </summary>
public class BookingRescheduledCalendarSyncHandler<TBooking> : IEventHandler<BookingRescheduledIntegrationEvent>
    where TBooking : BookingBase
{
    private readonly IRepository<TBooking, Guid> _bookings;
    private readonly IBookingCalendarGateway _calendar;

    public BookingRescheduledCalendarSyncHandler(IRepository<TBooking, Guid> bookings, IBookingCalendarGateway calendar)
    {
        _bookings = bookings;
        _calendar = calendar;
    }

    public async ValueTask HandleAsync(BookingRescheduledIntegrationEvent @event, CancellationToken ct = default)
    {
        var booking = await _bookings.GetByIdAsync(@event.BookingId, ct);
        if (booking?.CalendarEventId is { } eventId)
            await _calendar.RescheduleEventAsync(eventId, @event.StartUtc, @event.EndUtc, ct);
    }
}
