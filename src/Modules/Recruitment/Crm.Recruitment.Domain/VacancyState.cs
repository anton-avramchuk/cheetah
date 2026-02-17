using Cheetah.Core.Domain;
using Cheetah.Core.Domain.ValueObjects;

namespace Crm.Recruitment.Domain;

public class VacancyState : Entity<Guid>
{
    private readonly List<Vacancy> _vacancies = [];

    private VacancyState()
    {
    }

    public string Name { get; private set; } = null!;

    public int Order { get; private set; }

    public Color? Color { get; private set; }

    public bool IsDefault { get; private set; }

    public IReadOnlyCollection<Vacancy> Vacancies => _vacancies.AsReadOnly();

    public static VacancyState Create(string name, int order = 0, string? color = null, bool isDefault = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new VacancyState
        {
            Id = Guid.NewGuid(),
            Name = name,
            Order = order,
            Color = color is not null ? Color.Create(color) : null,
            IsDefault = isDefault
        };
    }

    public void Update(string name, int order, bool isDefault, string? color = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        Order = order;
        Color = color is not null ? Color.Create(color) : null;
        IsDefault = isDefault;
    }
}