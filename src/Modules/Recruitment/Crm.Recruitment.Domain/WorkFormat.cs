using Cheetah.Core.Domain;

namespace Crm.Recruitment.Domain;

public class WorkFormat : Entity<Guid>
{
    private readonly List<Vacancy> _vacancies = [];

    private WorkFormat()
    {
    }

    public string Name { get; private set; } = null!;

    public IReadOnlyCollection<Vacancy> Vacancies => _vacancies.AsReadOnly();

    public static WorkFormat Create(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new WorkFormat
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
