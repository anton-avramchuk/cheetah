using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Frontend.Events;

/// <summary>
/// In-memory implementation of IEventBus for frontend Blazor applications.
/// Events are handled synchronously within the same process.
/// </summary>
[Export(LifetimeType.Singleton, typeof(IEventBus))]
public sealed class CrmInMemoryEventBus : IEventBus
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<Type, List<Type>> _subscriptions = new();
    private readonly object _lock = new();

    public CrmInMemoryEventBus(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async ValueTask PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        var eventType = typeof(TEvent);
        List<Type> handlerTypes;

        lock (_lock)
        {
            if (!_subscriptions.TryGetValue(eventType, out var handlers))
            {
                // No handlers registered for this event type
                return;
            }

            handlerTypes = handlers.ToList();
        }

        await ExecuteHandlersAsync(@event, handlerTypes, cancellationToken);
    }

    public async ValueTask PublishManyAsync<TEvent>(IEnumerable<TEvent> events, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        var eventType = typeof(TEvent);
        List<Type> handlerTypes;

        lock (_lock)
        {
            if (!_subscriptions.TryGetValue(eventType, out var handlers))
            {
                // No handlers registered for this event type
                return;
            }

            handlerTypes = handlers.ToList();
        }

        // Execute handlers for each event sequentially
        foreach (var @event in events)
        {
            await ExecuteHandlersAsync(@event, handlerTypes, cancellationToken);
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
            if (!_subscriptions.ContainsKey(eventType))
            {
                _subscriptions[eventType] = new List<Type>();
            }

            if (!_subscriptions[eventType].Contains(handlerType))
            {
                _subscriptions[eventType].Add(handlerType);
            }
        }
    }

    private async ValueTask ExecuteHandlersAsync<TEvent>(
        TEvent @event,
        List<Type> handlerTypes,
        CancellationToken cancellationToken)
        where TEvent : IEvent
    {
        using var scope = _serviceProvider.CreateScope();

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
                    // For now, continue processing other handlers even if one fails
                }
            }
        }
    }
}
