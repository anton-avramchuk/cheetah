using Cheetah.Core.Domain;

namespace Crm.Recruitment.Domain;

public class Customer : Entity<Guid>
{
    private readonly List<Vacancy> _vacancies = [];

    private Customer()
    {
    }

    public string Name { get; private set; } = null!;

    public string? Code { get; private set; }

    public string? Description { get; private set; }

    public Guid? DirectionId { get; private set; }

    public CustomerDirection? Direction { get; private set; }

    public IReadOnlyCollection<Vacancy> Vacancies => _vacancies.AsReadOnly();

    public static Customer Create(string name, string? description = null, Guid? directionId = null, string? code = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Customer
        {
            Id = Guid.NewGuid(),
            Name = name,
            Code = code?.ToUpperInvariant(),
            Description = description,
            DirectionId = directionId
        };
    }

    public void Update(string name, string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        Description = description;
    }

    public void SetCode(string? code)
    {
        Code = code?.ToUpperInvariant();
    }

    public void SetDirection(Guid? directionId)
    {
        DirectionId = directionId;
    }
}
