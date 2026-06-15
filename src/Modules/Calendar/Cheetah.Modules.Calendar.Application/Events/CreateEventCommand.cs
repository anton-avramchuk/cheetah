using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Cheetah.Modules.Calendar.Application.Abstractions;
using Cheetah.Modules.Calendar.Application.Exceptions;
using Cheetah.Modules.Calendar.Application.Options;
using Cheetah.Modules.Calendar.Contracts;
using Cheetah.Modules.Calendar.Domain.Abstractions;
using Cheetah.Modules.Calendar.Domain.Entities;
using Cheetah.Modules.Calendar.Domain.Specifications;
using Cheetah.Modules.Calendar.Domain.ValueObjects;
using Microsoft.Extensions.Options;

namespace Cheetah.Modules.Calendar.Application.Events;

/// <summary>Создать событие (разовое или серию) с участниками и напоминаниями.</summary>
public sealed record CreateEventCommand(Guid CalendarId, CreateEventRequest Request) : ICommand<Guid>;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CreateEventCommand, Guid>))]
public sealed class CreateEventCommandHandler : ICommandHandler<CreateEventCommand, Guid>
{
    private readonly ICalendarEventRepository _events;
    private readonly IRepository<CalendarableEntityType, Guid> _types;
    private readonly IReminderScheduler _scheduler;
    private readonly IEventBus _eventBus;
    private readonly CalendarReminderOptions _options;

    public CreateEventCommandHandler(
        ICalendarEventRepository events,
        IRepository<CalendarableEntityType, Guid> types,
        IReminderScheduler scheduler,
        IEventBus eventBus,
        IOptions<CalendarReminderOptions> options)
    {
        _events = events;
        _types = types;
        _scheduler = scheduler;
        _eventBus = eventBus;
        _options = options.Value;
    }

    public async ValueTask<Guid> HandleAsync(CreateEventCommand command, CancellationToken ct = default)
    {
        var r = command.Request;

        if (!string.IsNullOrWhiteSpace(r.EntityType))
        {
            if (r.EntityId is null)
                throw new CalendarValidationException("EntityId is required when EntityType is set");
            if (!await _types.ExistsAsync(new CalendarableTypeByKeySpecification(r.EntityType), ct))
                throw new CalendarValidationException($"Entity type '{r.EntityType}' is not registered as calendarable");
        }

        var recurrence = string.IsNullOrWhiteSpace(r.RRule) ? null : new RecurrenceRule(r.RRule);

        var @event = CalendarEvent.Schedule(
            command.CalendarId, r.Title, r.StartUtc, r.EndUtc, r.OrganizerUserId,
            r.Description, r.Location, r.TimeZoneId, r.IsAllDay, r.EntityType, r.EntityId, recurrence);

        if (r.Attendees is not null)
            foreach (var a in r.Attendees)
                @event.AddAttendee(a.UserId, a.Role);

        if (r.Reminders is not null)
            foreach (var rem in r.Reminders)
                @event.AddReminder(rem.OffsetBeforeStart, rem.Target, rem.ForceChannel);

        _events.Add(@event);

        await _scheduler.RebuildAsync(@event, DateTime.UtcNow.AddDays(_options.HorizonDays), eventIsNew: true, cancellationToken: ct);

        foreach (var e in @event.DomainEvents)
            await _eventBus.PublishAsync(e, ct);
        @event.ClearDomainEvents();

        await _events.SaveChangesAsync(ct);
        return @event.Id;
    }
}
