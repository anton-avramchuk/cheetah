using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Cheetah.Modules.Calendar.Application.Abstractions;
using Cheetah.Modules.Calendar.Application.Events;
using Cheetah.Modules.Calendar.Application.Options;
using Cheetah.Modules.Calendar.Domain.Abstractions;
using Cheetah.Modules.Calendar.Shared;
using Microsoft.Extensions.Options;

namespace Cheetah.Modules.Calendar.Application.Reminders;

// Изменение набора напоминаний пересобирает материализованные триггеры события.

public sealed record AddReminderCommand(
    Guid EventId, TimeSpan OffsetBeforeStart, ReminderTarget Target, string? ForceChannel) : ICommand<Guid>;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<AddReminderCommand, Guid>))]
public sealed class AddReminderCommandHandler : ICommandHandler<AddReminderCommand, Guid>
{
    private readonly ICalendarEventRepository _events;
    private readonly IReminderScheduler _scheduler;
    private readonly IEventBus _eventBus;
    private readonly CalendarReminderOptions _options;

    public AddReminderCommandHandler(
        ICalendarEventRepository events, IReminderScheduler scheduler,
        IEventBus eventBus, IOptions<CalendarReminderOptions> options)
    {
        _events = events;
        _scheduler = scheduler;
        _eventBus = eventBus;
        _options = options.Value;
    }

    public async ValueTask<Guid> HandleAsync(AddReminderCommand command, CancellationToken ct = default)
    {
        var @event = await EventCommandShared.LoadAsync(_events, command.EventId, ct);
        var reminder = @event.AddReminder(
            command.OffsetBeforeStart, command.Target, command.ForceChannel);
        await _scheduler.RebuildAsync(@event, DateTime.UtcNow.AddDays(_options.HorizonDays), cancellationToken: ct);
        await EventCommandShared.PublishAndSaveAsync(_events, _eventBus, @event, ct);
        return reminder.Id;
    }
}

public sealed record RemoveReminderCommand(Guid EventId, Guid ReminderId) : ICommand;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<RemoveReminderCommand>))]
public sealed class RemoveReminderCommandHandler : ICommandHandler<RemoveReminderCommand>
{
    private readonly ICalendarEventRepository _events;
    private readonly IReminderScheduler _scheduler;
    private readonly IEventBus _eventBus;
    private readonly CalendarReminderOptions _options;

    public RemoveReminderCommandHandler(
        ICalendarEventRepository events, IReminderScheduler scheduler,
        IEventBus eventBus, IOptions<CalendarReminderOptions> options)
    {
        _events = events;
        _scheduler = scheduler;
        _eventBus = eventBus;
        _options = options.Value;
    }

    public async ValueTask HandleAsync(RemoveReminderCommand command, CancellationToken ct = default)
    {
        var @event = await EventCommandShared.LoadAsync(_events, command.EventId, ct);
        @event.RemoveReminder(command.ReminderId);
        await _scheduler.RebuildAsync(@event, DateTime.UtcNow.AddDays(_options.HorizonDays), cancellationToken: ct);
        await EventCommandShared.PublishAndSaveAsync(_events, _eventBus, @event, ct);
    }
}
