using System;
using Cheetah.Core.Domain;

namespace Crm.Identity.Domain;

public class UserIdentity : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    private UserIdentity()
    {
    } // For EF Core

    public static UserIdentity Create(string name, string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var entity = new UserIdentity
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description
        };

        // Uncomment when DomainEvents are needed:
        // entity.AddDomainEvent(new UserIdentityCreatedEvent(entity.Id, entity.Name));

        return entity;
    }

    public void Update(string name, string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        Description = description;
    }
}