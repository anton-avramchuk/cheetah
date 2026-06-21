using Cheetah.Core.Events;

namespace Cheetah.Modules.Calendar.DomainEvents;

/// <summary>
/// Событие запланировано. Публикуется ПОСЛЕ SaveChangesAsync. Привязка к сущности
/// (если есть) передаётся парой строк/Guid — DomainEvents не зависит от Shared.
/// </summary>
public record CalendarEventScheduledEvent(
    Guid CalendarEventId,
    Guid CalendarId,
    string Title,
    DateTime StartUtc,
    DateTime EndUtc,
    string? EntityType,
    Guid? EntityId,
    Guid OrganizerUserId) : EventBase;

/// <summary>Событие перенесено (изменён период/таймзона).</summary>
public record CalendarEventRescheduledEvent(
    Guid CalendarEventId,
    DateTime StartUtc,
    DateTime EndUtc,
    string TimeZoneId) : EventBase;

/// <summary>Реквизиты события изменены (заголовок/описание/локация).</summary>
public record CalendarEventDetailsChangedEvent(
    Guid CalendarEventId,
    string Title) : EventBase;

/// <summary>Правило повторения установлено или изменено.</summary>
public record CalendarEventRecurrenceChangedEvent(
    Guid CalendarEventId,
    string? RRule) : EventBase;

/// <summary>Событие отменено.</summary>
public record CalendarEventCancelledEvent(
    Guid CalendarEventId) : EventBase;
