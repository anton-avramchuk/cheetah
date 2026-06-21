using Cheetah.Core.Events;

namespace Cheetah.Modules.Calendar.DomainEvents;

/// <summary>Участник приглашён на событие.</summary>
public record EventAttendeeInvitedEvent(
    Guid CalendarEventId,
    Guid AttendeeUserId,
    string Role) : EventBase;

/// <summary>Участник ответил на приглашение (RSVP).</summary>
public record EventAttendeeRespondedEvent(
    Guid CalendarEventId,
    Guid AttendeeUserId,
    string Response) : EventBase;
