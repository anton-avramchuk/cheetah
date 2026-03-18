using Cheetah.Core.Domain;

namespace AppName.Domain;

public class SampleEntity : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    private SampleEntity() { } // For EF Core

    public static SampleEntity Create(string name, string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var entity = new SampleEntity
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description
        };

        // Uncomment when DomainEvents are needed:
        // entity.AddDomainEvent(new SampleEntityCreatedEvent(entity.Id, entity.Name));

        return entity;
    }

    public void Update(string name, string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        Description = description;
    }
}
