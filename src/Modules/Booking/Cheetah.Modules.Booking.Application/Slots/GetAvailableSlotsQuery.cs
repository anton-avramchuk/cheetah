using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Modules.Booking.Application.Abstractions;
using Cheetah.Modules.Booking.Contracts;
using Cheetah.Modules.Booking.Domain.Entities;
using Cheetah.Modules.Booking.Domain.Slots;
using Cheetah.Modules.Booking.Domain.Specifications;

namespace Cheetah.Modules.Booking.Application.Slots;

/// <summary>Свободные слоты публичной страницы записи в окне дат, в таймзоне invitee.</summary>
public sealed record GetAvailableSlotsQuery(string Slug, DateOnly From, DateOnly To, string InviteeTimeZone)
    : IQuery<IReadOnlyList<SlotDto>>;

public class GetAvailableSlotsQueryHandler<TBookingType, TSchedule>
    : IQueryHandler<GetAvailableSlotsQuery, IReadOnlyList<SlotDto>>
    where TBookingType : BookingTypeBase
    where TSchedule : AvailabilityScheduleBase
{
    private readonly IRepository<TBookingType, Guid> _types;
    private readonly IRepository<TSchedule, Guid> _schedules;
    private readonly IBookingCalendarGateway _calendar;
    private readonly ISlotEngine _engine;

    public GetAvailableSlotsQueryHandler(
        IRepository<TBookingType, Guid> types, IRepository<TSchedule, Guid> schedules,
        IBookingCalendarGateway calendar, ISlotEngine engine)
    {
        _types = types;
        _schedules = schedules;
        _calendar = calendar;
        _engine = engine;
    }

    public async ValueTask<IReadOnlyList<SlotDto>> HandleAsync(GetAvailableSlotsQuery query, CancellationToken ct = default)
    {
        var type = await _types.GetBySpecAsync(new BookingTypeBySlugSpecification<TBookingType>(query.Slug), ct);
        if (type is null)
            return Array.Empty<SlotDto>();

        var schedule = await _schedules.GetBySpecAsync(new ScheduleByHostSpecification<TSchedule>(type.HostUserId), ct);
        if (schedule is null)
            return Array.Empty<SlotDto>();

        var fromUtc = new DateTimeOffset(query.From.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var toUtc = new DateTimeOffset(query.To.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

        var busy = await _calendar.GetBusyAsync(type.HostUserId, fromUtc, toUtc, ct);
        var slots = _engine.ComputeSlots(type, schedule, busy, fromUtc, toUtc, DateTimeOffset.UtcNow);

        var inviteeTz = ResolveTimeZone(query.InviteeTimeZone);
        return slots
            .Select(s => new SlotDto(s.StartUtc, s.EndUtc, RenderLocal(s.StartUtc, inviteeTz)))
            .ToArray();
    }

    private static TimeZoneInfo ResolveTimeZone(string id)
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
        catch (TimeZoneNotFoundException) { return TimeZoneInfo.Utc; }
        catch (InvalidTimeZoneException) { return TimeZoneInfo.Utc; }
    }

    private static string RenderLocal(DateTimeOffset utc, TimeZoneInfo tz)
        => TimeZoneInfo.ConvertTime(utc, tz).ToString("yyyy-MM-ddTHH:mm:ss");
}
