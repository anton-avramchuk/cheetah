using Cheetah.Core.Domain;
using Cheetah.Core.Domain.ValueObjects;

namespace Crm.VacancyTasks.Domain;

public class TaskState : Entity<Guid>
{
    private readonly List<VacancyTask> _vacancyTasks = [];

    private TaskState()
    {
    }

    public string Name { get; private set; } = null!;

    public int Order { get; private set; }

    public Color? Color { get; private set; }

    public bool IsDefault { get; private set; }

    public IReadOnlyCollection<VacancyTask> VacancyTasks => _vacancyTasks.AsReadOnly();

    public static TaskState Create(string name, int order = 0, string? color = null, bool isDefault = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new TaskState
        {
            Id = Guid.NewGuid(),
            Name = name,
            Order = order,
            Color = color is not null ? Color.Create(color) : null,
            IsDefault = isDefault
        };
    }

    public void Update(string name, int order, string? color = null, bool isDefault = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        Order = order;
        Color = color is not null ? Color.Create(color) : null;
        IsDefault = isDefault;
    }
}
