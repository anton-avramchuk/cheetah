using Cheetah.Core.Domain;
using Cheetah.Modules.Calendar.Domain.ValueObjects;
using Cheetah.Modules.Calendar.DomainEvents;
using Cheetah.Modules.Calendar.Shared;

namespace Cheetah.Modules.Calendar.Domain.Entities;

/// <summary>
/// Событие календаря — агрегат. Хранит период в UTC + IANA-таймзону (RRULE раскрывается в TZ
/// события). Может быть разовым или серией (<see cref="Recurrence"/>), привязанным к сущности
/// другого модуля парой <see cref="EntityType"/>/<see cref="EntityId"/>. Участники, напоминания
/// и override-ы экземпляров — часть инвариантной границы агрегата.
/// </summary>
public class CalendarEvent : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity, IRemovedAtEntity
{
    public Guid CalendarId { get; private set; }

    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public string? Location { get; private set; }

    public DateTime StartUtc { get; private set; }
    public DateTime EndUtc { get; private set; }
    public string TimeZoneId { get; private set; } = "UTC";
    public bool IsAllDay { get; private set; }

    public string? EntityType { get; private set; }
    public Guid? EntityId { get; private set; }

    public RecurrenceRule? Recurrence { get; private set; }
    public bool IsRecurring => Recurrence is not null;

    public Guid OrganizerUserId { get; private set; }
    public EventStatus Status { get; private set; }

    private readonly List<EventAttendee> _attendees = new();
    private readonly List<EventReminder> _reminders = new();
    private readonly List<EventOccurrenceOverride> _overrides = new();
    public IReadOnlyCollection<EventAttendee> Attendees => _attendees.AsReadOnly();
    public IReadOnlyCollection<EventReminder> Reminders => _reminders.AsReadOnly();
    public IReadOnlyCollection<EventOccurrenceOverride> Overrides => _overrides.AsReadOnly();

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? RemovedAt { get; private set; }

    private CalendarEvent() { } // EF

    public static CalendarEvent Schedule(
        Guid calendarId,
        string title,
        DateTime startUtc,
        DateTime endUtc,
        Guid organizerUserId,
        string? description = null,
        string? location = null,
        string timeZoneId = "UTC",
        bool isAllDay = false,
        string? entityType = null,
        Guid? entityId = null,
        RecurrenceRule? recurrence = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        startUtc = AsUtc(startUtc);
        endUtc = AsUtc(endUtc);
        if (endUtc < startUtc)
            throw new ArgumentException("Event end cannot be earlier than start", nameof(endUtc));

        var @event = new CalendarEvent
        {
            Id = Guid.NewGuid(),
            CalendarId = calendarId,
            Title = title.Trim(),
            Description = description,
            Location = location,
            StartUtc = startUtc,
            EndUtc = endUtc,
            TimeZoneId = string.IsNullOrWhiteSpace(timeZoneId) ? "UTC" : timeZoneId,
            IsAllDay = isAllDay,
            EntityType = entityType,
            EntityId = entityId,
            Recurrence = recurrence,
            OrganizerUserId = organizerUserId,
            Status = EventStatus.Confirmed
        };

        // Организатор всегда участник.
        @event._attendees.Add(EventAttendee.Create(@event.Id, organizerUserId, AttendeeRole.Organizer));

        @event.AddDomainEvent(new CalendarEventScheduledEvent(
            @event.Id, calendarId, @event.Title, startUtc, endUtc, entityType, entityId, organizerUserId));
        return @event;
    }

    public void ChangeDetails(string title, string? description, string? location)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        Title = title.Trim();
        Description = description;
        Location = location;
        AddDomainEvent(new CalendarEventDetailsChangedEvent(Id, Title));
    }

    public void Reschedule(DateTime startUtc, DateTime endUtc, string timeZoneId, bool isAllDay)
    {
        startUtc = AsUtc(startUtc);
        endUtc = AsUtc(endUtc);
        if (endUtc < startUtc)
            throw new ArgumentException("Event end cannot be earlier than start", nameof(endUtc));

        StartUtc = startUtc;
        EndUtc = endUtc;
        TimeZoneId = string.IsNullOrWhiteSpace(timeZoneId) ? "UTC" : timeZoneId;
        IsAllDay = isAllDay;
        AddDomainEvent(new CalendarEventRescheduledEvent(Id, startUtc, endUtc, TimeZoneId));
    }

    public void LinkToEntity(string entityType, Guid entityId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
        EntityType = entityType.Trim();
        EntityId = entityId;
    }

    public void UnlinkEntity()
    {
        EntityType = null;
        EntityId = null;
    }

    public void SetRecurrence(RecurrenceRule? recurrence)
    {
        Recurrence = recurrence;
        AddDomainEvent(new CalendarEventRecurrenceChangedEvent(Id, recurrence?.RRule));
    }

    public EventAttendee AddAttendee(Guid userId, AttendeeRole role)
    {
        var existing = _attendees.FirstOrDefault(a => a.UserId == userId);
        if (existing is not null)
            return existing;

        var attendee = EventAttendee.Create(Id, userId, role);
        _attendees.Add(attendee);
        AddDomainEvent(new EventAttendeeInvitedEvent(Id, userId, role.ToString()));
        return attendee;
    }

    public void RemoveAttendee(Guid userId)
    {
        var attendee = _attendees.FirstOrDefault(a => a.UserId == userId);
        if (attendee is not null && attendee.Role != AttendeeRole.Organizer)
            _attendees.Remove(attendee);
    }

    public void RespondToInvite(Guid userId, AttendeeResponse response)
    {
        var attendee = _attendees.FirstOrDefault(a => a.UserId == userId)
            ?? throw new InvalidOperationException($"User {userId} is not an attendee of event {Id}");
        attendee.Respond(response);
        AddDomainEvent(new EventAttendeeRespondedEvent(Id, userId, response.ToString()));
    }

    public EventReminder AddReminder(TimeSpan offsetBeforeStart, ReminderTarget target, string? forceChannel)
    {
        var reminder = EventReminder.Create(Id, offsetBeforeStart, target, forceChannel);
        _reminders.Add(reminder);
        return reminder;
    }

    public void RemoveReminder(Guid reminderId)
    {
        var reminder = _reminders.FirstOrDefault(r => r.Id == reminderId);
        if (reminder is not null)
            _reminders.Remove(reminder);
    }

    /// <summary>Отменить один экземпляр серии (EXDATE-подобная отмена через override).</summary>
    public void CancelOccurrence(string occurrenceKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(occurrenceKey);
        if (_overrides.Any(o => o.OccurrenceKey == occurrenceKey))
            return;
        _overrides.Add(EventOccurrenceOverride.Cancellation(Id, occurrenceKey));
    }

    /// <summary>Переопределить время/заголовок одного экземпляра серии.</summary>
    public void OverrideOccurrence(string occurrenceKey, DateTime? newStartUtc, DateTime? newEndUtc, string? newTitle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(occurrenceKey);
        _overrides.RemoveAll(o => o.OccurrenceKey == occurrenceKey);
        _overrides.Add(EventOccurrenceOverride.Modification(Id, occurrenceKey, newStartUtc, newEndUtc, newTitle));
    }

    public void Cancel()
    {
        if (Status == EventStatus.Cancelled)
            return;
        Status = EventStatus.Cancelled;
        RemovedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new CalendarEventCancelledEvent(Id));
    }

    private static DateTime AsUtc(DateTime value) => DateTime.SpecifyKind(value, DateTimeKind.Utc);
}
