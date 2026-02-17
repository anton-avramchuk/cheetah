using Cheetah.Core.Domain;

namespace Crm.VacancyTasks.Domain;

public class VacancyTask : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public string Title { get; private set; } = null!;

    public string? Description { get; private set; }

    public Guid VacancyId { get; private set; }

    public Guid StateId { get; private set; }

    public TaskState State { get; private set; } = null!;

    public Guid? PriorityId { get; private set; }

    public TaskPriority? Priority { get; private set; }

    public Guid? AssigneeId { get; private set; }

    public DateTimeOffset? DueDate { get; private set; }

    public int Order { get; private set; }

    public int Number { get; private set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    private VacancyTask()
    {
    }

    public static VacancyTask Create(
        string title,
        Guid vacancyId,
        Guid stateId,
        int number,
        string? description = null,
        Guid? priorityId = null,
        Guid? assigneeId = null,
        DateTimeOffset? dueDate = null,
        int order = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        return new VacancyTask
        {
            Id = Guid.NewGuid(),
            Title = title,
            VacancyId = vacancyId,
            StateId = stateId,
            Number = number,
            Description = description,
            PriorityId = priorityId,
            AssigneeId = assigneeId,
            DueDate = dueDate,
            Order = order
        };
    }

    public void Update(string title, string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        Title = title;
        Description = description;
    }

    public void SetState(Guid stateId)
    {
        StateId = stateId;
    }

    public void SetPriority(Guid? priorityId)
    {
        PriorityId = priorityId;
    }

    public void SetAssignee(Guid? assigneeId)
    {
        AssigneeId = assigneeId;
    }

    public void SetDueDate(DateTimeOffset? dueDate)
    {
        DueDate = dueDate;
    }

    public void Move(Guid stateId, int order)
    {
        StateId = stateId;
        Order = order;
    }
}
