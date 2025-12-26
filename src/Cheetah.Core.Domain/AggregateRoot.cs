using Cheetah.Core.Events;

namespace Cheetah.Core.Domain;

/// <summary>
/// Root entity of an aggregate that can raise domain events
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId>
{
    private readonly List<IEvent> _domainEvents = new();

    public IReadOnlyCollection<IEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected AggregateRoot()
    {
    }

    protected AggregateRoot(TId id) : base(id)
    {
    }

    protected void AddDomainEvent(IEvent @event)
    {
        _domainEvents.Add(@event);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

public abstract class AggregateRoot : AggregateRoot<Guid>
{
    protected AggregateRoot(Guid id) : base(id)
    {
    }

    protected AggregateRoot()
    {
    }
}