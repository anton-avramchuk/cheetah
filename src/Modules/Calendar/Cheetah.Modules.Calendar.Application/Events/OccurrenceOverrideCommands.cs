using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Cheetah.Modules.Calendar.Application.Abstractions;
using Cheetah.Modules.Calendar.Application.Options;
using Cheetah.Modules.Calendar.Domain.Abstractions;
using Microsoft.Extensions.Options;

namespace Cheetah.Modules.Calendar.Application.Events;

// Отмена/переопределение одного экземпляра серии (RECURRENCE-ID). Влияет на материализованные
// напоминания этого экземпляра → пересобираем триггеры.

public sealed record CancelOccurrenceCommand(Guid EventId, string OccurrenceKey) : ICommand;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<CancelOccurrenceCommand>))]
public sealed class CancelOccurrenceCommandHandler : ICommandHandler<CancelOccurrenceCommand>
{
    private readonly ICalendarEventRepository _events;
    private readonly IReminderScheduler _scheduler;
    private readonly IEventBus _eventBus;
    private readonly CalendarReminderOptions _options;

    public CancelOccurrenceCommandHandler(
        ICalendarEventRepository events, IReminderScheduler scheduler,
        IEventBus eventBus, IOptions<CalendarReminderOptions> options)
    {
        _events = events;
        _scheduler = scheduler;
        _eventBus = eventBus;
        _options = options.Value;
    }

    public async ValueTask HandleAsync(CancelOccurrenceCommand command, CancellationToken ct = default)
    {
        var @event = await EventCommandShared.LoadAsync(_events, command.EventId, ct);
        @event.CancelOccurrence(command.OccurrenceKey);
        await _scheduler.RebuildAsync(@event, DateTime.UtcNow.AddDays(_options.HorizonDays), ct);
        await EventCommandShared.PublishAndSaveAsync(_events, _eventBus, @event, ct);
    }
}

public sealed record OverrideOccurrenceCommand(
    Guid EventId,
    string OccurrenceKey,
    DateTime? NewStartUtc,
    DateTime? NewEndUtc,
    string? NewTitle) : ICommand;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<OverrideOccurrenceCommand>))]
public sealed class OverrideOccurrenceCommandHandler : ICommandHandler<OverrideOccurrenceCommand>
{
    private readonly ICalendarEventRepository _events;
    private readonly IReminderScheduler _scheduler;
    private readonly IEventBus _eventBus;
    private readonly CalendarReminderOptions _options;

    public OverrideOccurrenceCommandHandler(
        ICalendarEventRepository events, IReminderScheduler scheduler,
        IEventBus eventBus, IOptions<CalendarReminderOptions> options)
    {
        _events = events;
        _scheduler = scheduler;
        _eventBus = eventBus;
        _options = options.Value;
    }

    public async ValueTask HandleAsync(OverrideOccurrenceCommand command, CancellationToken ct = default)
    {
        var @event = await EventCommandShared.LoadAsync(_events, command.EventId, ct);
        @event.OverrideOccurrence(command.OccurrenceKey, command.NewStartUtc, command.NewEndUtc, command.NewTitle);
        await _scheduler.RebuildAsync(@event, DateTime.UtcNow.AddDays(_options.HorizonDays), ct);
        await EventCommandShared.PublishAndSaveAsync(_events, _eventBus, @event, ct);
    }
}
