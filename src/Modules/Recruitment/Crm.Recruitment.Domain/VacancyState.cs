using Cheetah.Core.Domain;

namespace Crm.Recruitment.Domain;

public class VacancyState : Entity<Guid>
{
    private readonly List<Vacancy> _vacancies = [];

    private VacancyState()
    {
    }

    public string Name { get; private set; } = null!;

    public int Order { get; private set; }

    public IReadOnlyCollection<Vacancy> Vacancies => _vacancies.AsReadOnly();

    public static VacancyState Create(string name, int order = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new VacancyState
        {
            Id = Guid.NewGuid(),
            Name = name,
            Order = order
        };
    }

    public void Update(string name, int order)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        Order = order;
    }
}