using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Backend.Events.InMemory;

[Export(LifetimeType.Singleton, typeof(IEventBus))]
public sealed class InMemoryEventBus(IServiceProvider serviceProvider) : IEventBus
{
    private readonly Dictionary<Type, List<Type>> _subscriptions = new();
    private readonly Lock _lock = new();

    public async ValueTask PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        await HandleEventAsync(@event, cancellationToken);
    }

    public async ValueTask PublishManyAsync<TEvent>(IEnumerable<TEvent> events, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        foreach (var @event in events)
        {
            await HandleEventAsync(@event, cancellationToken);
        }
    }

    public void Subscribe<TEvent, THandler>()
        where TEvent : IEvent
        where THandler : IEventHandler<TEvent>
    {
        var eventType = typeof(TEvent);
        var handlerType = typeof(THandler);

        lock (_lock)
        {
            if (!_subscriptions.TryGetValue(eventType, out var handlers))
            {
                handlers = new List<Type>();
                _subscriptions[eventType] = handlers;
            }

            if (!handlers.Contains(handlerType))
            {
                handlers.Add(handlerType);
            }
        }
    }

    private async Task HandleEventAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)
        where TEvent : IEvent
    {
        List<Type> handlerTypes;
        lock (_lock)
        {
            if (!_subscriptions.TryGetValue(typeof(TEvent), out var handlers))
                return;

            handlerTypes = handlers.ToList();
        }

        using var scope = serviceProvider.CreateScope();

        foreach (var handlerType in handlerTypes)
        {
            var handler = scope.ServiceProvider.GetService(handlerType);
            if (handler is IEventHandler<TEvent> eventHandler)
            {
                try
                {
                    await eventHandler.HandleAsync(@event, cancellationToken);
                }
                catch
                {
                    // TODO: Add logging
                }
            }
        }
    }
}
