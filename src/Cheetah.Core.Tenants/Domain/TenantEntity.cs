using Cheetah.Core.Domain;
using Cheetah.Core.Tenants.Events;

namespace Cheetah.Core.Tenants.Domain;

public abstract class TenantEntity<TTenantCreatedEvent, TTenantUpdatedEvent, TTenantDeactivatedEvent, TTenantActivatedEvent>
    : AggregateRoot, ICreateAtEntity, IUpdatedAtEntity, IRemovedAtEntity
    where TTenantCreatedEvent : TenantCreatedEvent
    where TTenantUpdatedEvent : TenantUpdatedEvent
    where TTenantDeactivatedEvent : TenantDeactivatedEvent
    where TTenantActivatedEvent : TenantActivatedEvent
{
    public string Name { get; protected set; } = null!;
    public string? Description { get; protected set; }
    public bool IsActive { get; protected set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public DateTimeOffset? RemovedAt { get; protected set; }

    protected TenantEntity() : base()
    {
    }

    protected TenantEntity(Guid id, string name) : base(id)
    {
        Name = name;
    }

    protected abstract TTenantCreatedEvent CreateTenantCreatedEvent(Guid tenantId, string name);
    protected abstract TTenantUpdatedEvent CreateTenantUpdatedEvent(Guid tenantId, string name);
    protected abstract TTenantDeactivatedEvent CreateTenantDeactivatedEvent(Guid tenantId);
    protected abstract TTenantActivatedEvent CreateTenantActivatedEvent(Guid tenantId);
}