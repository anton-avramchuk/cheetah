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

    public Guid? CustomerId { get; private set; }

    public Customer? Customer { get; private set; }

    public Guid? PositionId { get; private set; }

    public Position? Position { get; private set; }

    public Guid? StackItemId { get; private set; }

    public StackItem? StackItem { get; private set; }

    public Guid? WorkFormatId { get; private set; }

    public WorkFormat? WorkFormat { get; private set; }

    public int Order { get; private set; }

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

    public void SetCustomer(Guid? customerId)
    {
        CustomerId = customerId;
    }

    public void SetPosition(Guid? positionId)
    {
        PositionId = positionId;
    }

    public void SetStackItem(Guid? stackItemId)
    {
        StackItemId = stackItemId;
    }

    public void SetWorkFormat(Guid? workFormatId)
    {
        WorkFormatId = workFormatId;
    }

    public void Move(Guid stateId, int order)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(order);

        StateId = stateId;
        Order = order;
    }
}
