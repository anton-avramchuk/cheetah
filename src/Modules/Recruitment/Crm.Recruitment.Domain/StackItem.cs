using Cheetah.Core.Domain;

namespace Crm.Recruitment.Domain;

public class StackItem : Entity<Guid>
{
    private readonly List<Vacancy> _vacancies = [];

    private StackItem()
    {
    }

    public string Name { get; private set; } = null!;

    public IReadOnlyCollection<Vacancy> Vacancies => _vacancies.AsReadOnly();

    public static StackItem Create(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new StackItem
        {
            Id = Guid.NewGuid(),
            Name = name
        };
    }

    public void Update(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
    }
}
