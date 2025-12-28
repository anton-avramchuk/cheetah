using Cheetah.Core.Events;

namespace Cheetah.Tenants.Domain.Events;

public record TenantActivatedEvent(Guid TenantId) : IEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
