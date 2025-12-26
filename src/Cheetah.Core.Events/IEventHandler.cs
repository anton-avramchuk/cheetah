namespace Cheetah.Core.Events;

/// <summary>
/// Handler for processing events
/// </summary>
/// <typeparam name="TEvent">Type of event to handle</typeparam>
public interface IEventHandler<in TEvent> where TEvent : IEvent
{
    /// <summary>
    /// Handles the event asynchronously
    /// </summary>
    ValueTask HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
}