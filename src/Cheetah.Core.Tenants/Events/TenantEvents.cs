using Cheetah.Core.Events;

namespace Cheetah.Core.Tenants.Events;

public record TenantCreatedEvent(Guid TenantId, string Name) : EventBase;
public record TenantUpdatedEvent(Guid TenantId, string Name, bool IsActive) : EventBase;
public record TenantDeletedEvent(Guid TenantId) : EventBase;