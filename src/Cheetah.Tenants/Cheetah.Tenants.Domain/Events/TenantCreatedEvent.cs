using Cheetah.Core.Events;

namespace Cheetah.Tenants.Domain.Events;

public record TenantCreatedEvent(
    Guid TenantId,
    string TenantName,
    string? Subdomain
) : IEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
