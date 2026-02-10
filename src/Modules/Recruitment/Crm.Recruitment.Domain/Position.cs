using Cheetah.Core.Domain;

namespace Crm.Recruitment.Domain;

public class Position : Entity<Guid>
{
    private readonly List<Vacancy> _vacancies = [];

    private Position()
    {
    }

    public string Name { get; private set; } = null!;

    public IReadOnlyCollection<Vacancy> Vacancies => _vacancies.AsReadOnly();

    public static Position Create(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Position
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
