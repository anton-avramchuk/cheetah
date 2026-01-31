using Cheetah.Core.Domain;

namespace Cheetah.Admin.Modules.Clients.Domain;

public class Tariff : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public string Name { get; private set; } = null!;

    public string? Description { get; private set; }

    public decimal Price { get; private set; }

    public string Currency { get; private set; } = null!;

    public bool IsActive { get; private set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    private Tariff() { } // For EF Core

    public static Tariff Create(
        string name,
        decimal price,
        string currency,
        string? description = null,
        bool isActive = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        if (currency.Length != 3)
            throw new ArgumentException("Currency must be a 3-character ISO code", nameof(currency));

        if (price < 0)
            throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative");

        var tariff = new Tariff
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            Price = price,
            Currency = currency.ToUpperInvariant(),
            IsActive = isActive
        };

        // TODO: Add domain event when Events project is created
        // tariff.AddDomainEvent(new TariffCreatedEvent(tariff.Id, tariff.Name));

        return tariff;
    }

    public void Update(
        string name,
        decimal price,
        string currency,
        string? description,
        bool isActive)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        if (currency.Length != 3)
            throw new ArgumentException("Currency must be a 3-character ISO code", nameof(currency));

        if (price < 0)
            throw new ArgumentOutOfRangeException(nameof(price), "Price cannot be negative");

        Name = name;
        Description = description;
        Price = price;
        Currency = currency.ToUpperInvariant();
        IsActive = isActive;

        // TODO: Add domain event when Events project is created
        // AddDomainEvent(new TariffUpdatedEvent(Id, Name));
    }
}
