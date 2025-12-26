namespace Cheetah.Core.Events;

/// <summary>
/// Event bus for publishing and subscribing to events across modules
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publishes a single event
    /// </summary>
    ValueTask PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) 
        where TEvent : IEvent;
    
    /// <summary>
    /// Publishes multiple events in batch
    /// </summary>
    ValueTask PublishManyAsync<TEvent>(IEnumerable<TEvent> events, CancellationToken cancellationToken = default)
        where TEvent : IEvent;
    
    /// <summary>
    /// Subscribes a handler to an event type
    /// </summary>
    void Subscribe<TEvent, THandler>()
        where TEvent : IEvent
        where THandler : IEventHandler<TEvent>;
}