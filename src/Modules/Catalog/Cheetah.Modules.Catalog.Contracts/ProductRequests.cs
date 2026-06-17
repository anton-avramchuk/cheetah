using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Cheetah.Modules.Catalog.Shared;

namespace Cheetah.Modules.Catalog.Contracts;

/// <summary>
/// Базовый запрос на создание товара. Абстрактен: наследник объявляет конкретный
/// <c>sealed record CreateProductRequest : CreateProductRequestBase</c>, добавляет свои поля и
/// (опционально) атрибут <c>[ApiRoute(..., ApiMethod.Create)]</c>.
/// </summary>
public abstract record CreateProductRequestBase : ICrmRequest
{
    public string Sku { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public ProductType Type { get; init; }
    public UnitOfMeasure Unit { get; init; } = UnitOfMeasure.Piece;
    public Guid? CategoryId { get; init; }
}

/// <summary>Базовый запрос на обновление базовых полей товара (Id — из маршрута).</summary>
public abstract record UpdateProductRequestBase : ICrmRequest
{
    [FromRoute] public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public Guid? CategoryId { get; init; }
}

/// <summary>Запрос «товар по Id».</summary>
public abstract record GetProductByIdRequestBase : ICrmRequest
{
    [FromRoute] public Guid Id { get; init; }
}

/// <summary>Запрос «удалить товар».</summary>
public abstract record DeleteProductRequestBase : ICrmRequest
{
    [FromRoute] public Guid Id { get; init; }
}

/// <summary>Запрос грида товаров (пагинация/сортировка/фильтрация). Наследник — конкретный class.</summary>
public abstract class GetProductsGridRequestBase : GridRequest;
