using Cheetah.Core.Domain;
using Cheetah.Core.Domain.ValueObjects;

namespace Crm.VacancyTasks.Domain;

public class TaskPriority : Entity<Guid>
{
    private readonly List<VacancyTask> _vacancyTasks = [];

    private TaskPriority()
    {
    }

    public string Name { get; private set; } = null!;

    public int Order { get; private set; }

    public Color? Color { get; private set; }

    public IReadOnlyCollection<VacancyTask> VacancyTasks => _vacancyTasks.AsReadOnly();

    public static TaskPriority Create(string name, int order = 0, string? color = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new TaskPriority
        {
            Id = Guid.NewGuid(),
            Name = name,
            Order = order,
            Color = color is not null ? Color.Create(color) : null
        };
    }

    public void Update(string name, int order, string? color = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        Order = order;
        Color = color is not null ? Color.Create(color) : null;
    }
}
