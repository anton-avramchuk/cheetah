using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Cheetah.Modules.Calendar.Shared;

namespace Cheetah.Modules.Calendar.Contracts;

// Маршрутные поля помечены [FromRoute]: для GET/DELETE их подтягивает [AsParameters] по имени,
// для POST/PATCH — MergeRouteValuesInto в сгенерированном эндпоинте. Тело запроса их не несёт.

// ── Календари ────────────────────────────────────────────────────────────────────────────

/// <summary>Создание календаря.</summary>
public sealed record CreateCalendarRequest(
    string Name,
    CalendarType Type,
    Guid OwnerUserId,
    string DefaultTimeZoneId = "UTC",
    string? Color = null) : ICrmRequest;

/// <summary>Календарь по идентификатору.</summary>
public sealed record GetCalendarByIdRequest(
    [property: FromRoute] Guid CalendarId) : ICrmRequest;

/// <summary>События календаря, попадающие в окно [FromUtc, ToUtc).</summary>
public sealed record ListEventsByCalendarRequest(
    [property: FromRoute] Guid CalendarId,
    DateTime FromUtc,
    DateTime ToUtc) : ICrmRequest;

// ── Участники/напоминания при создании события ───────────────────────────────────────────

/// <summary>Участник при создании события.</summary>
public sealed record CreateAttendeeRequest(
    Guid UserId,
    AttendeeRole Role = AttendeeRole.Required);

/// <summary>Напоминание при создании события.</summary>
public sealed record CreateReminderRequest(
    TimeSpan OffsetBeforeStart,
    ReminderTarget Target = ReminderTarget.AllAttendees,
    string? ForceChannel = null);

// ── События ──────────────────────────────────────────────────────────────────────────────

/// <summary>Создание события. RRule — необязательное правило повторения (RFC 5545).</summary>
public sealed record CreateEventRequest(
    [property: FromRoute] Guid CalendarId,
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
    IReadOnlyList<CreateReminderRequest>? Reminders = null) : ICrmRequest;

/// <summary>Событие по идентификатору со всеми участниками и напоминаниями.</summary>
public sealed record GetEventByIdRequest(
    [property: FromRoute] Guid EventId) : ICrmRequest;

/// <summary>Изменение реквизитов события.</summary>
public sealed record UpdateEventDetailsRequest(
    [property: FromRoute] Guid EventId,
    string Title,
    string? Description = null,
    string? Location = null) : ICrmRequest;

/// <summary>Перенос события.</summary>
public sealed record RescheduleEventRequest(
    [property: FromRoute] Guid EventId,
    DateTime StartUtc,
    DateTime EndUtc,
    string TimeZoneId = "UTC",
    bool IsAllDay = false) : ICrmRequest;

/// <summary>Установка/очистка правила повторения.</summary>
public sealed record SetRecurrenceRequest(
    [property: FromRoute] Guid EventId,
    string? RRule,
    IReadOnlyList<DateTime>? ExDatesUtc = null) : ICrmRequest;

/// <summary>Отмена события (мягкая).</summary>
public sealed record CancelEventRequest(
    [property: FromRoute] Guid EventId) : ICrmRequest;

/// <summary>События, привязанные к сущности, в окне [FromUtc, ToUtc).</summary>
public sealed record ListEventsByEntityRequest(
    [property: FromRoute] string EntityType,
    [property: FromRoute] Guid EntityId,
    DateTime FromUtc,
    DateTime ToUtc) : ICrmRequest;

/// <summary>Повестка пользователя в окне [FromUtc, ToUtc).</summary>
public sealed record ListAgendaRequest(
    [property: FromRoute] Guid UserId,
    DateTime FromUtc,
    DateTime ToUtc) : ICrmRequest;

// ── Экземпляры серии ─────────────────────────────────────────────────────────────────────

/// <summary>Отмена одного экземпляра серии.</summary>
public sealed record CancelOccurrenceRequest(
    [property: FromRoute] Guid EventId,
    [property: FromRoute] string OccurrenceKey) : ICrmRequest;

/// <summary>Переопределение одного экземпляра серии (время/заголовок).</summary>
public sealed record OverrideOccurrenceRequest(
    [property: FromRoute] Guid EventId,
    [property: FromRoute] string OccurrenceKey,
    DateTime? NewStartUtc = null,
    DateTime? NewEndUtc = null,
    string? NewTitle = null) : ICrmRequest;

// ── Участники ────────────────────────────────────────────────────────────────────────────

/// <summary>Добавление участника.</summary>
public sealed record AddAttendeeRequest(
    [property: FromRoute] Guid EventId,
    Guid UserId,
    AttendeeRole Role = AttendeeRole.Required) : ICrmRequest;

/// <summary>Ответ на приглашение (RSVP).</summary>
public sealed record RespondToInviteRequest(
    [property: FromRoute] Guid EventId,
    Guid UserId,
    AttendeeResponse Response) : ICrmRequest;

// ── Напоминания ──────────────────────────────────────────────────────────────────────────

/// <summary>Добавление напоминания.</summary>
public sealed record AddReminderRequest(
    [property: FromRoute] Guid EventId,
    TimeSpan OffsetBeforeStart,
    ReminderTarget Target = ReminderTarget.AllAttendees,
    string? ForceChannel = null) : ICrmRequest;

/// <summary>Удаление напоминания.</summary>
public sealed record RemoveReminderRequest(
    [property: FromRoute] Guid EventId,
    [property: FromRoute] Guid ReminderId) : ICrmRequest;

// ── Реестр привязываемых типов ───────────────────────────────────────────────────────────

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
    IReadOnlyList<CalendarableEntityTypeRegistration> Items) : ICrmRequest;

/// <summary>Все зарегистрированные привязываемые типы (без параметров).</summary>
public sealed record GetCalendarableTypesRequest : ICrmRequest;
