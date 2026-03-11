using Cheetah.Core.Domain;

namespace Crm.Customer.Domain;

public class Customer : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    private Customer()
    {
    } // For EF Core

    public static Customer Create(string name, string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var entity = new Customer
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description
        };

        // Uncomment when DomainEvents are needed:
        // entity.AddDomainEvent(new CustomerCreatedEvent(entity.Id, entity.Name));

        return entity;
    }

    public void Update(string name, string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        Description = description;
    }
}