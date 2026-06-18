using Cheetah.Modules.Booking.Application.Abstractions;
using Cheetah.Modules.Booking.Domain.Entities;
using Cheetah.Modules.Booking.Domain.Slots;
using Cheetah.Modules.Booking.Shared;
using Cheetah.Modules.Calendar.Client;
using Cheetah.Modules.Calendar.Contracts;

namespace Cheetah.Modules.Booking.Infrastructure.Calendar;

/// <summary>
/// Реализация <see cref="IBookingCalendarGateway"/> поверх <c>Calendar.Client</c>: занятость host'а
/// берётся из free/busy Calendar, итоговое событие создаётся в календаре, разрешённом
/// <see cref="IHostCalendarResolver"/>. Так Application не зависит от Calendar.Client напрямую.
/// </summary>
public sealed class BookingCalendarGateway : IBookingCalendarGateway
{
    private readonly ICalendarClient _calendar;
    private readonly IHostCalendarResolver _calendars;

    public BookingCalendarGateway(ICalendarClient calendar, IHostCalendarResolver calendars)
    {
        _calendar = calendar;
        _calendars = calendars;
    }

    public async ValueTask<IReadOnlyList<BusyInterval>> GetBusyAsync(
        Guid hostUserId, DateTimeOffset fromUtc, DateTimeOffset toUtc, CancellationToken ct = default)
    {
        var busy = await _calendar.GetUserBusyAsync(hostUserId, fromUtc, toUtc, ct);
        return busy
            .Select(b => new BusyInterval(
                new DateTimeOffset(DateTime.SpecifyKind(b.StartUtc, DateTimeKind.Unspecified), TimeSpan.Zero),
                new DateTimeOffset(DateTime.SpecifyKind(b.EndUtc, DateTimeKind.Unspecified), TimeSpan.Zero)))
            .ToArray();
    }

    public async ValueTask<Guid?> CreateEventAsync(BookingBase booking, BookingTypeBase type, CancellationToken ct = default)
    {
        var calendarId = await _calendars.ResolveAsync(booking.HostUserId, ct);
        if (calendarId is not { } cid)
            return null;

        var request = new CreateEventRequest(
            CalendarId: cid,
            Title: $"{type.Name} — {booking.InviteeName}",
            StartUtc: booking.StartUtc.UtcDateTime,
            EndUtc: booking.EndUtc.UtcDateTime,
            OrganizerUserId: booking.HostUserId,
            Description: $"{booking.InviteeName} <{booking.InviteeEmail}>",
            Location: type.LocationDetails,
            EntityType: EntityRefKeys.Booking,
            EntityId: booking.Id);

        return await _calendar.CreateEventAsync(cid, request, ct);
    }
}
