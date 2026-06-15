using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Cheetah.Modules.Calendar.Application.Events;
using Cheetah.Modules.Calendar.Domain.Abstractions;
using Cheetah.Modules.Calendar.Shared;

namespace Cheetah.Modules.Calendar.Application.Attendees;

// Изменение состава/ответов участников не пересобирает триггеры: получатели резолвятся
// в момент отправки по Target, а не материализуются заранее.

public sealed record AddAttendeeCommand(Guid EventId, Guid UserId, AttendeeRole Role) : ICommand;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<AddAttendeeCommand>))]
public sealed class AddAttendeeCommandHandler : ICommandHandler<AddAttendeeCommand>
{
    private readonly ICalendarEventRepository _events;
    private readonly IEventBus _eventBus;

    public AddAttendeeCommandHandler(ICalendarEventRepository events, IEventBus eventBus)
    {
        _events = events;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(AddAttendeeCommand command, CancellationToken ct = default)
    {
        var @event = await EventCommandShared.LoadAsync(_events, command.EventId, ct);
        @event.AddAttendee(command.UserId, command.Role);
        await EventCommandShared.PublishAndSaveAsync(_events, _eventBus, @event, ct);
    }
}

public sealed record RespondToInviteCommand(Guid EventId, Guid UserId, AttendeeResponse Response) : ICommand;

[Export(LifetimeType.Scoped, typeof(ICommandHandler<RespondToInviteCommand>))]
public sealed class RespondToInviteCommandHandler : ICommandHandler<RespondToInviteCommand>
{
    private readonly ICalendarEventRepository _events;
    private readonly IEventBus _eventBus;

    public RespondToInviteCommandHandler(ICalendarEventRepository events, IEventBus eventBus)
    {
        _events = events;
        _eventBus = eventBus;
    }

    public async ValueTask HandleAsync(RespondToInviteCommand command, CancellationToken ct = default)
    {
        var @event = await EventCommandShared.LoadAsync(_events, command.EventId, ct);
        @event.RespondToInvite(command.UserId, command.Response);
        await EventCommandShared.PublishAndSaveAsync(_events, _eventBus, @event, ct);
    }
}
