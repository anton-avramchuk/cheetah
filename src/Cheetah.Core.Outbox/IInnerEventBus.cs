using Cheetah.Core.Events;

namespace Cheetah.Core.Outbox;

/// <summary>
/// Маркер реального транспорта событий (Redis/InMemory/etc.).
/// OutboxProcessor использует его для фактической отправки.
/// Регистрация выполняется CrmOutboxModule через адаптер.
/// </summary>
public interface IInnerEventBus : IEventBus
{
}

internal sealed class InnerEventBusAdapter : IInnerEventBus
{
    private readonly IEventBus _inner;

    public InnerEventBusAdapter(IEventBus inner) => _inner = inner;

    public ValueTask PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent
        => _inner.PublishAsync(@event, cancellationToken);

    public ValueTask PublishManyAsync<TEvent>(IEnumerable<TEvent> events, CancellationToken cancellationToken = default)
        where TEvent : IEvent
        => _inner.PublishManyAsync(events, cancellationToken);

    public void Subscribe<TEvent, THandler>()
        where TEvent : IEvent
        where THandler : IEventHandler<TEvent>
        => _inner.Subscribe<TEvent, THandler>();
}
