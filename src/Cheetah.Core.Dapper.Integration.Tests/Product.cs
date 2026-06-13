using Cheetah.Core.Dapper.Mapping;
using Cheetah.Core.Domain;
using Cheetah.Core.Events;

namespace Cheetah.Core.Dapper.Integration.Tests;

/// <summary>Aggregate used by the integration tests; raises a domain event on creation
/// so we also verify that <see cref="AggregateRoot{TId}.DomainEvents"/> is excluded from SQL.</summary>
public sealed class Product : AggregateRoot<Guid>
{
    public string Name { get; private set; } = null!;
    public decimal Price { get; private set; }
    public int Stock { get; private set; }
    public bool IsActive { get; private set; }

    private Product() { }

    public static Product Create(string name, decimal price, int stock)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = name,
            Price = price,
            Stock = stock,
            IsActive = true,
        };
        product.AddDomainEvent(new ProductCreatedEvent(product.Id));
        return product;
    }

    public static Product CreateWithId(Guid id, string name, decimal price, int stock)
    {
        var product = Create(name, price, stock);
        product.Id = id;
        return product;
    }

    public void Deactivate() => IsActive = false;

    public void SetStock(int stock) => Stock = stock;
}

public sealed record ProductCreatedEvent(Guid ProductId) : EventBase;

/// <summary>Maps <see cref="Product"/> to the snake_case <c>products</c> table.</summary>
public sealed class ProductMap : DapperEntityMap<Product>
{
    public ProductMap()
    {
        ToTable("products");
        HasKey(x => x.Id);
        Column(x => x.Id, "id");
        Column(x => x.Name, "name");
        Column(x => x.Price, "price");
        Column(x => x.Stock, "stock");
        Column(x => x.IsActive, "is_active");
    }
}
