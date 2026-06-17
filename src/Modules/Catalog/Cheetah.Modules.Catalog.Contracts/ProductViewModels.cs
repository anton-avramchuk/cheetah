using Cheetah.Contracts.Responses;
using Cheetah.Modules.Catalog.Shared;

namespace Cheetah.Modules.Catalog.Contracts;

/// <summary>
/// Базовый «лёгкий» ViewModel товара для грида (без тяжёлых полей). Абстрактен: наследник объявляет
/// <c>sealed record ProductGridViewModel : ProductGridViewModelBase</c> и добавляет свои колонки.
/// Точка расширяемости табличного представления.
/// </summary>
public abstract record ProductGridViewModelBase : ICrmResponse
{
    public Guid Id { get; init; }
    public string Sku { get; init; } = null!;
    public string Name { get; init; } = null!;
    public ProductType Type { get; init; }
    public UnitOfMeasure Unit { get; init; }
    public Guid? CategoryId { get; init; }
    public bool IsActive { get; init; }
}
