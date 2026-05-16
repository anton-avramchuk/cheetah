using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;

namespace Cheetah.Core.Outbox;

/// <summary>
/// Декоратор IEventBus: PublishAsync складывает событие в IOutboxStore (в той же транзакции,
/// что и доменные изменения), а Subscribe делегируется реальному транспорту.
/// Регистрируется как Scoped в CrmOutboxModule поверх IInnerEventBus.
/// </summary>
[Export(LifetimeType.Scoped, typeof(IEventBus))]
public sealed class OutboxEventBus : IEventBus
{
    private readonly IOutboxStore _store;
    private readonly IInnerEventBus _inner;

    public OutboxEventBus(IOutboxStore store, IInnerEventBus inner)
    {
        _store = store;
        _inner = inner;
    }

    public async ValueTask PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        var message = OutboxEventSerializer.Serialize(@event);
        await _store.AddAsync(message, cancellationToken);
    }

    public async ValueTask PublishManyAsync<TEvent>(IEnumerable<TEvent> events, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        foreach (var @event in events)
        {
            var message = OutboxEventSerializer.Serialize(@event);
            await _store.AddAsync(message, cancellationToken);
        }
    }

    public void Subscribe<TEvent, THandler>()
        where TEvent : IEvent
        where THandler : IEventHandler<TEvent>
        => _inner.Subscribe<TEvent, THandler>();
}
