using Cheetah.Contracts.Responses;
using Cheetah.Modules.Calendar.Shared;

namespace Cheetah.Modules.Calendar.Contracts;

/// <summary>Календарь — контейнер событий.</summary>
public sealed record CalendarDto(
    Guid Id,
    string Name,
    CalendarType Type,
    Guid OwnerUserId,
    string DefaultTimeZoneId,
    string? Color,
    DateTimeOffset? CreatedAt) : ICrmResponse;

/// <summary>Событие календаря с участниками и напоминаниями.</summary>
public sealed record CalendarEventDto(
    Guid Id,
    Guid CalendarId,
    string Title,
    string? Description,
    string? Location,
    DateTime StartUtc,
    DateTime EndUtc,
    string TimeZoneId,
    bool IsAllDay,
    string? EntityType,
    Guid? EntityId,
    string? RRule,
    EventStatus Status,
    Guid OrganizerUserId,
    IReadOnlyList<EventAttendeeDto> Attendees,
    IReadOnlyList<EventReminderDto> Reminders) : ICrmResponse;

/// <summary>Участник события.</summary>
public sealed record EventAttendeeDto(
    Guid Id,
    Guid UserId,
    AttendeeRole Role,
    AttendeeResponse Response);

/// <summary>Правило напоминания.</summary>
public sealed record EventReminderDto(
    Guid Id,
    TimeSpan OffsetBeforeStart,
    ReminderTarget Target,
    string? ForceChannel);

/// <summary>
/// Конкретный экземпляр события на временной шкале (после раскрытия серии RRULE).
/// Для разового события — один экземпляр с тем же периодом, что и само событие.
/// </summary>
public sealed record EventOccurrenceDto(
    Guid EventId,
    string Title,
    string? Location,
    DateTime StartUtc,
    DateTime EndUtc,
    bool IsAllDay,
    string OccurrenceKey,
    bool IsOverride) : ICrmResponse;

/// <summary>Зарегистрированный тип сущности, к которой можно привязывать события.</summary>
public sealed record CalendarableEntityTypeDto(
    string EntityType,
    string DisplayName,
    string? DefaultColor,
    bool AllowMultiplePerEntity,
    string? OwnerService) : ICrmResponse;
