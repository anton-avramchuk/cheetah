using Cheetah.Modules.Calendar.Shared;

namespace Cheetah.Modules.Calendar.Contracts;

/// <summary>Создание календаря.</summary>
public sealed record CreateCalendarRequest(
    string Name,
    CalendarType Type,
    Guid OwnerUserId,
    string DefaultTimeZoneId = "UTC",
    string? Color = null);

/// <summary>Участник при создании события.</summary>
public sealed record CreateAttendeeRequest(
    Guid UserId,
    AttendeeRole Role = AttendeeRole.Required);

/// <summary>Напоминание при создании события.</summary>
public sealed record CreateReminderRequest(
    TimeSpan OffsetBeforeStart,
    ReminderTarget Target = ReminderTarget.AllAttendees,
    string? ForceChannel = null);

/// <summary>Создание события. RRule — необязательное правило повторения (RFC 5545).</summary>
public sealed record CreateEventRequest(
    string Title,
    DateTime StartUtc,
    DateTime EndUtc,
    Guid OrganizerUserId,
    string? Description = null,
    string? Location = null,
    string TimeZoneId = "UTC",
    bool IsAllDay = false,
    string? EntityType = null,
    Guid? EntityId = null,
    string? RRule = null,
    IReadOnlyList<CreateAttendeeRequest>? Attendees = null,
    IReadOnlyList<CreateReminderRequest>? Reminders = null);

/// <summary>Изменение реквизитов события.</summary>
public sealed record UpdateEventDetailsRequest(
    string Title,
    string? Description = null,
    string? Location = null);

/// <summary>Перенос события.</summary>
public sealed record RescheduleEventRequest(
    DateTime StartUtc,
    DateTime EndUtc,
    string TimeZoneId = "UTC",
    bool IsAllDay = false);

/// <summary>Установка/очистка правила повторения.</summary>
public sealed record SetRecurrenceRequest(
    string? RRule,
    IReadOnlyList<DateTime>? ExDatesUtc = null);

/// <summary>Добавление участника.</summary>
public sealed record AddAttendeeRequest(
    Guid UserId,
    AttendeeRole Role = AttendeeRole.Required);

/// <summary>Ответ на приглашение (RSVP).</summary>
public sealed record RespondToInviteRequest(
    Guid UserId,
    AttendeeResponse Response);

/// <summary>Добавление напоминания.</summary>
public sealed record AddReminderRequest(
    TimeSpan OffsetBeforeStart,
    ReminderTarget Target = ReminderTarget.AllAttendees,
    string? ForceChannel = null);

/// <summary>Переопределение одного экземпляра серии (время/заголовок).</summary>
public sealed record OverrideOccurrenceRequest(
    DateTime? NewStartUtc = null,
    DateTime? NewEndUtc = null,
    string? NewTitle = null);

/// <summary>Регистрация одного привязываемого типа сущности.</summary>
public sealed record CalendarableEntityTypeRegistration(
    string EntityType,
    string DisplayName,
    string? DefaultColor = null,
    bool AllowMultiplePerEntity = true,
    string? OwnerService = null);

/// <summary>Пакетная синхронизация реестра привязываемых типов (идемпотентный upsert).</summary>
public sealed record CalendarRegistrySyncRequest(
    string OwnerService,
    IReadOnlyList<CalendarableEntityTypeRegistration> Items);
