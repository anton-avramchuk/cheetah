using Cheetah.Modules.Catalog.Domain.Entities;
using Cheetah.Modules.Catalog.Shared;

namespace Cheetah.Modules.Catalog.Domain.Tests;

/// <summary>
/// Конкретный наследник <see cref="ProductBase"/> для проверки базового поведения. Доп. поле
/// <see cref="Brand"/> демонстрирует расширяемость сущности.
/// </summary>
public sealed class TestProduct : ProductBase
{
    public string? Brand { get; private set; }

    private TestProduct() { }

    public static TestProduct Create(
        string sku, string name, ProductType type = ProductType.Goods,
        UnitOfMeasure unit = UnitOfMeasure.Piece, Guid? categoryId = null,
        string? description = null, string? brand = null)
    {
        var product = new TestProduct();
        product.InitializeCore(Guid.NewGuid(), sku, name, type, unit, categoryId, description);
        product.Brand = brand;
        return product;
    }
}
