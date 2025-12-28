using Cheetah.Core.Domain;
using Cheetah.Core.Tenants.Events;

namespace Cheetah.Core.Tenants.Domain;

public class Tenant : AggregateRoot, ICreateAtEntity, IUpdatedAtEntity,IRemovedAtEntity
{
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset? CreatedAt { get; set; }
    
    public DateTimeOffset? UpdatedAt { get; set; }
    
    public DateTimeOffset? RemovedAt { get; private set; }

    private Tenant()
    {
    } // For EF Core

    public Tenant(string name, string? description = null) : base(Guid.NewGuid())
    {
        Name = name;
        Description = description;
        IsActive = true;
        AddDomainEvent(new TenantCreatedEvent(Id, Name));
    }

    public void Update(string name, string? description, bool isActive)
    {
        Name = name;
        Description = description;
        IsActive = isActive;
        AddDomainEvent(new TenantUpdatedEvent(Id, Name, IsActive));
    }

    public void Delete()
    {
        IsActive = false;
        RemovedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new TenantDeletedEvent(Id));
    }
}