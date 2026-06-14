using Cheetah.Modules.Calendar.Contracts;
using Cheetah.Modules.Calendar.Domain.Abstractions;
using Cheetah.Modules.Calendar.Domain.Entities;

namespace Cheetah.Modules.Calendar.Application.Mapping;

/// <summary>
/// Проекция доменных сущностей в DTO. Без Mapster — VO <c>RecurrenceRule</c> и коллекции детей
/// раскрываются явно, чтобы не требовать скрытой конфигурации маппинга.
/// </summary>
internal static class CalendarProjector
{
    public static CalendarDto ToDto(Domain.Entities.Calendar c)
        => new(c.Id, c.Name, c.Type, c.OwnerUserId, c.DefaultTimeZoneId, c.Color, c.CreatedAt);

    public static CalendarEventDto ToDto(CalendarEvent e)
        => new(
            e.Id, e.CalendarId, e.Title, e.Description, e.Location,
            e.StartUtc, e.EndUtc, e.TimeZoneId, e.IsAllDay,
            e.EntityType, e.EntityId, e.Recurrence?.RRule, e.Status, e.OrganizerUserId,
            e.Attendees.Select(ToDto).ToArray(),
            e.Reminders.Select(ToDto).ToArray());

    public static EventAttendeeDto ToDto(EventAttendee a)
        => new(a.Id, a.UserId, a.Role, a.Response);

    public static EventReminderDto ToDto(EventReminder r)
        => new(r.Id, r.OffsetBeforeStart, r.Target, r.ForceChannel);

    public static EventOccurrenceDto ToOccurrenceDto(CalendarEvent e, Occurrence occ)
        => new(e.Id, e.Title, e.Location, occ.StartUtc, occ.EndUtc, e.IsAllDay, occ.OccurrenceKey, occ.IsOverride);

    public static CalendarableEntityTypeDto ToDto(CalendarableEntityType t)
        => new(t.EntityType, t.DisplayName, t.DefaultColor, t.AllowMultiplePerEntity, t.OwnerService);
}
