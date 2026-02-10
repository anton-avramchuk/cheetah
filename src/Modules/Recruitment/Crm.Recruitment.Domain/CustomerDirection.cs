using Cheetah.Core.Domain;

namespace Crm.Recruitment.Domain;

public class CustomerDirection : Entity<Guid>
{
    private readonly List<Customer> _customers = [];

    private CustomerDirection()
    {
    }

    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public IReadOnlyCollection<Customer> Customers => _customers.AsReadOnly();

    public static CustomerDirection Create(string name, string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new CustomerDirection
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description
        };
    }

    public void Update(string name, string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        Description = description;
    }
}
