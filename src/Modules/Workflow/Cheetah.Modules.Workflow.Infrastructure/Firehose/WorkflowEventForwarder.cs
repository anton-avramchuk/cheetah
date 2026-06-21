using System.Text.Json;
using Cheetah.Core.Events;
using Cheetah.Workflow;

namespace Cheetah.Modules.Workflow.Infrastructure.Firehose;

/// <summary>
/// Декоратор <see cref="IEventBus"/>: при публикации любого <see cref="EventBase"/> дополнительно шлёт его
/// «копию» в firehose-канал как <see cref="WorkflowEventEnvelope"/>. Прозрачно для источников — им не нужно
/// знать о Workflow. Сам конверт не дублируется (защита от рекурсии). См. план §2.2.
/// </summary>
public sealed class WorkflowEventForwarder : IEventBus
{
    private readonly IEventBus _inner;

    public WorkflowEventForwarder(IEventBus inner) => _inner = inner;

    public async ValueTask PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        await _inner.PublishAsync(@event, cancellationToken);
        if (@event is EventBase eb and not WorkflowEventEnvelope)
            await _inner.PublishAsync(ToEnvelope(eb), cancellationToken);
    }

    public async ValueTask PublishManyAsync<TEvent>(IEnumerable<TEvent> events, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        foreach (var @event in events)
            await PublishAsync(@event, cancellationToken);
    }

    public void Subscribe<TEvent, THandler>()
        where TEvent : IEvent
        where THandler : IEventHandler<TEvent>
        => _inner.Subscribe<TEvent, THandler>();

    private static WorkflowEventEnvelope ToEnvelope(EventBase e)
    {
        var json = JsonSerializer.Serialize(e, e.GetType());
        var payload = JsonSerializer.Deserialize<Dictionary<string, object?>>(json) ?? new Dictionary<string, object?>();
        return new WorkflowEventEnvelope(e.GetType().Name, e.EventId, payload)
        {
            OccurredAt = e.OccurredAt
        };
    }
}
