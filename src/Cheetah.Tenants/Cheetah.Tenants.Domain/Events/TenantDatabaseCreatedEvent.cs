using Cheetah.Core.Events;

namespace Cheetah.Tenants.Domain.Events;

public record TenantDatabaseCreatedEvent(
    Guid TenantId,
    string DatabaseName,
    string ConnectionString
) : IEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
