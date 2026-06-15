using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Cheetah.Modules.Calendar.Application.Abstractions;
using Cheetah.Modules.Calendar.Application.Exceptions;
using Cheetah.Modules.Calendar.Application.Options;
using Cheetah.Modules.Calendar.Domain.Abstractions;
using Cheetah.Modules.Calendar.Domain.Entities;
using Cheetah.Modules.Calendar.Domain.ValueObjects;
using Microsoft.Extensions.Options;

namespace Cheetah.Modules.Calendar.Application.Events;

// ── Изменение реквизитов (не влияет на тайминг → напоминания не пересобираем) ──────────────

public sealed record UpdateEventDetailsCommand(
    Guid EventId, string Title, string? Description, string? Location) : ICommand;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<UpdateEventDetailsCommand>))]
public sealed class UpdateEventDetailsCommandHandler : ICommandHandler<UpdateEventDetailsCommand>
{
    private readonly ICalendarEventRepository _events;
    private readonly IEventBus _eventBus;

    public UpdateEventDetailsCommandHandler(ICalendarEventRepository events, IEventBus eventBus)
    {
        _events = events;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(UpdateEventDetailsCommand command, CancellationToken ct = default)
    {
        var @event = await Load(_events, command.EventId, ct);
        @event.ChangeDetails(command.Title, command.Description, command.Location);
        await EventCommandShared.PublishAndSaveAsync(_events, _eventBus, @event, ct);
    }

    private static ValueTask<CalendarEvent> Load(ICalendarEventRepository r, Guid id, CancellationToken ct)
        => EventCommandShared.LoadAsync(r, id, ct);
}

// ── Перенос (меняет тайминг → пересобираем напоминания) ──────────────────────────────────

public sealed record RescheduleEventCommand(
    Guid EventId, DateTime StartUtc, DateTime EndUtc, string TimeZoneId, bool IsAllDay) : ICommand;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<RescheduleEventCommand>))]
public sealed class RescheduleEventCommandHandler : ICommandHandler<RescheduleEventCommand>
{
    private readonly ICalendarEventRepository _events;
    private readonly IReminderScheduler _scheduler;
    private readonly IEventBus _eventBus;
    private readonly CalendarReminderOptions _options;

    public RescheduleEventCommandHandler(
        ICalendarEventRepository events, IReminderScheduler scheduler,
        IEventBus eventBus, IOptions<CalendarReminderOptions> options)
    {
        _events = events;
        _scheduler = scheduler;
        _eventBus = eventBus;
        _options = options.Value;
    }

    public async ValueTask HandleAsync(RescheduleEventCommand command, CancellationToken ct = default)
    {
        var @event = await EventCommandShared.LoadAsync(_events, command.EventId, ct);
        @event.Reschedule(command.StartUtc, command.EndUtc, command.TimeZoneId, command.IsAllDay);
        await _scheduler.RebuildAsync(@event, DateTime.UtcNow.AddDays(_options.HorizonDays), cancellationToken: ct);
        await EventCommandShared.PublishAndSaveAsync(_events, _eventBus, @event, ct);
    }
}

// ── Установка/очистка правила повторения ─────────────────────────────────────────────────

public sealed record SetEventRecurrenceCommand(
    Guid EventId, string? RRule, IReadOnlyList<DateTime>? ExDatesUtc) : ICommand;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<SetEventRecurrenceCommand>))]
public sealed class SetEventRecurrenceCommandHandler : ICommandHandler<SetEventRecurrenceCommand>
{
    private readonly ICalendarEventRepository _events;
    private readonly IReminderScheduler _scheduler;
    private readonly IEventBus _eventBus;
    private readonly CalendarReminderOptions _options;

    public SetEventRecurrenceCommandHandler(
        ICalendarEventRepository events, IReminderScheduler scheduler,
        IEventBus eventBus, IOptions<CalendarReminderOptions> options)
    {
        _events = events;
        _scheduler = scheduler;
        _eventBus = eventBus;
        _options = options.Value;
    }

    public async ValueTask HandleAsync(SetEventRecurrenceCommand command, CancellationToken ct = default)
    {
        var @event = await EventCommandShared.LoadAsync(_events, command.EventId, ct);
        var rule = string.IsNullOrWhiteSpace(command.RRule)
            ? null
            : new RecurrenceRule(command.RRule, command.ExDatesUtc);
        @event.SetRecurrence(rule);
        await _scheduler.RebuildAsync(@event, DateTime.UtcNow.AddDays(_options.HorizonDays), cancellationToken: ct);
        await EventCommandShared.PublishAndSaveAsync(_events, _eventBus, @event, ct);
    }
}

// ── Отмена ───────────────────────────────────────────────────────────────────────────────

public sealed record CancelEventCommand(Guid EventId) : ICommand;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CancelEventCommand>))]
public sealed class CancelEventCommandHandler : ICommandHandler<CancelEventCommand>
{
    private readonly ICalendarEventRepository _events;
    private readonly IReminderScheduler _scheduler;
    private readonly IEventBus _eventBus;
    private readonly CalendarReminderOptions _options;

    public CancelEventCommandHandler(
        ICalendarEventRepository events, IReminderScheduler scheduler,
        IEventBus eventBus, IOptions<CalendarReminderOptions> options)
    {
        _events = events;
        _scheduler = scheduler;
        _eventBus = eventBus;
        _options = options.Value;
    }

    public async ValueTask HandleAsync(CancelEventCommand command, CancellationToken ct = default)
    {
        var @event = await EventCommandShared.LoadAsync(_events, command.EventId, ct);
        @event.Cancel();
        // Отменённое событие → гасим все ожидающие напоминания.
        await _scheduler.RebuildAsync(@event, DateTime.UtcNow.AddDays(_options.HorizonDays), cancellationToken: ct);
        await EventCommandShared.PublishAndSaveAsync(_events, _eventBus, @event, ct);
    }
}

// ── Общие шаги обработчиков событий ─────────────────────────────────────────────────────

internal static class EventCommandShared
{
    public static async ValueTask<CalendarEvent> LoadAsync(ICalendarEventRepository events, Guid id, CancellationToken ct)
        => await events.GetWithDetailsAsync(id, ct)
           ?? throw new CalendarValidationException($"Event {id} not found");

    public static async ValueTask PublishAndSaveAsync(
        ICalendarEventRepository events, IEventBus eventBus, CalendarEvent @event, CancellationToken ct)
    {
        foreach (var e in @event.DomainEvents)
            await eventBus.PublishAsync(e, ct);
        @event.ClearDomainEvents();
        await events.SaveChangesAsync(ct);
    }
}
