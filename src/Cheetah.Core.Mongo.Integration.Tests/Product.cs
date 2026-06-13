using System.Linq.Expressions;
using Cheetah.Core.Domain;
using Cheetah.Core.Specification;

namespace Cheetah.Core.Mongo.Integration.Tests;

/// <summary>Aggregate persisted by the Mongo repository in the integration tests.</summary>
public class Product : AggregateRoot<Guid>
{
    public string Name { get; private set; } = null!;
    public decimal Price { get; private set; }
    public int Stock { get; private set; }
    public bool IsActive { get; private set; }

    private Product() { }

    public static Product Create(string name, decimal price, int stock, bool isActive = true)
        => new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Price = price,
            Stock = stock,
            IsActive = isActive,
        };

    public void Restock(int quantity) => Stock += quantity;
}

public sealed class ProductByNameSpecification(string name) : Specification<Product>
{
    public override Expression<Func<Product, bool>> ToExpression() => p => p.Name == name;
}

public sealed class ActiveProductsSpecification : Specification<Product>
{
    public override Expression<Func<Product, bool>> ToExpression() => p => p.IsActive;
}
