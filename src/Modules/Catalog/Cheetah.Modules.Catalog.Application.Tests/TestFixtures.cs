using Cheetah.Modules.Catalog.Application.Abstractions;
using Cheetah.Modules.Catalog.Contracts;
using Cheetah.Modules.Catalog.Domain.Entities;
using Cheetah.Modules.Catalog.Shared;

namespace Cheetah.Modules.Catalog.Application.Tests;

/// <summary>Конкретный товар наследника с доп. полем — для проверки generic-хендлеров.</summary>
public sealed class TestProduct : ProductBase
{
    public string? Brand { get; private set; }

    private TestProduct() { }

    public static TestProduct Create(TestCreateRequest r)
    {
        var p = new TestProduct();
        p.InitializeCore(Guid.NewGuid(), r.Sku, r.Name, r.Type, r.Unit, r.CategoryId, r.Description);
        p.Brand = r.Brand;
        return p;
    }
}

public sealed record TestCreateRequest : CreateProductRequestBase
{
    public string? Brand { get; init; }
}

public sealed record TestUpdateRequest : UpdateProductRequestBase;

public sealed record TestProductDto : ProductDtoBase
{
    public string? Brand { get; init; }
}

public sealed record TestProductGridViewModel : ProductGridViewModelBase
{
    public string? Brand { get; init; }
}

public sealed class TestProductFactory : IProductFactory<TestProduct, TestCreateRequest>
{
    public TestProduct Create(TestCreateRequest request) => TestProduct.Create(request);
}

public sealed class TestProductProjector : IProductProjector<TestProduct, TestProductDto>
{
    public TestProductDto ToDto(TestProduct p) => new()
    {
        Id = p.Id,
        Sku = p.Sku,
        Name = p.Name,
        Description = p.Description,
        Type = p.Type,
        Unit = p.Unit,
        CategoryId = p.CategoryId,
        IsActive = p.IsActive,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt,
        Brand = p.Brand
    };
}

internal static class TestData
{
    public static TestCreateRequest CreateRequest(string sku = "SKU-1", string name = "Widget") => new()
    {
        Sku = sku,
        Name = name,
        Type = ProductType.Goods,
        Unit = UnitOfMeasure.Piece
    };

    public static TestProduct NewProduct() => TestProduct.Create(CreateRequest());
}
