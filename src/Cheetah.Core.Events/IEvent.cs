namespace Cheetah.Core.Events;

/// <summary>
/// Base marker interface for all events in the system
/// </summary>
public interface IEvent
{
    /// <summary>
    /// Unique identifier for this event instance
    /// </summary>
    Guid EventId { get; }
    
    /// <summary>
    /// Timestamp when the event occurred
    /// </summary>
    DateTimeOffset OccurredAt { get; }
}