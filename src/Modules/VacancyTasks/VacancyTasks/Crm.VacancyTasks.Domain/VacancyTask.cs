using Cheetah.Core.Domain;

namespace Crm.VacancyTasks.Domain;

public class VacancyTask : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    private VacancyTask()
    {
    } // For EF Core

    public static VacancyTask Create(string name, string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var entity = new VacancyTask
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description
        };

        // Uncomment when DomainEvents are needed:
        // entity.AddDomainEvent(new VacancyTaskCreatedEvent(entity.Id, entity.Name));

        return entity;
    }

    public void Update(string name, string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        Description = description;
    }
}