using Cheetah.Contracts.Responses;
using Cheetah.Modules.Catalog.Shared;

namespace Cheetah.Modules.Catalog.Contracts;

/// <summary>
/// Базовый ViewModel товара (граница API). Абстрактен: наследник объявляет конкретный
/// <c>sealed record ProductDto : ProductDtoBase</c> и при необходимости добавляет свои поля
/// (например, <c>Brand</c>, <c>Barcode</c>, <c>WeightKg</c>). Это и есть точка расширяемости ViewModel.
/// </summary>
public abstract record ProductDtoBase : ICrmResponse
{
    public Guid Id { get; init; }
    public string Sku { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public ProductType Type { get; init; }
    public UnitOfMeasure Unit { get; init; }
    public Guid? CategoryId { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}
