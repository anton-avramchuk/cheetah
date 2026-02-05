using Cheetah.Core.Domain;

namespace Crm.Recruitment.Domain;

public class Vacancy : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public Guid? StateId { get; private set; }

    public VacancyState? State { get; private set; }

    private Vacancy()
    {
    } // For EF Core

    public static Vacancy Create(string name, string? description = null, Guid? stateId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var entity = new Vacancy
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            StateId = stateId
        };

        // Uncomment when DomainEvents are needed:
        // entity.AddDomainEvent(new VacancyCreatedEvent(entity.Id, entity.Name));

        return entity;
    }

    public void Update(string name, string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        Description = description;
    }

    public void SetState(Guid? stateId)
    {
        StateId = stateId;
    }
}