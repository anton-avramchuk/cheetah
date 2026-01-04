using Cheetah.Core.Events;

namespace Cheetah.Core.Tenants.Events;

public abstract record TenantCreatedEvent(Guid TenantId, string Name) : EventBase;

public abstract record TenantUpdatedEvent(Guid TenantId, string Name) : EventBase;

public abstract record TenantDeactivatedEvent(Guid TenantId) : EventBase;

public abstract record TenantActivatedEvent(Guid TenantId) : EventBase;