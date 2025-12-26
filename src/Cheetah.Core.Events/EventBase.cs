namespace Cheetah.Core.Events;

/// <summary>
/// Base class for all events providing common properties
/// </summary>
public abstract record EventBase : IEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}