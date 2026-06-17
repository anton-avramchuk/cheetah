namespace Cheetah.Modules.SalesDocuments.Domain.Abstractions;

/// <summary>Снимок цены и наименования товара из каталога на момент добавления строки.</summary>
public sealed record ProductPriceSnapshot(string Name, decimal UnitPrice);

/// <summary>
/// Порт получения цены/наименования товара из каталога. Объявлен в Domain, реализуется адаптером над
/// <c>Catalog.Client</c> (в Infrastructure/хосте). Если адаптер не подключён, возвращает <c>null</c> —
/// тогда строка добавляется только с явной ценой (<c>UnitPriceOverride</c>) из запроса.
/// </summary>
public interface IProductPricingPort
{
    ValueTask<ProductPriceSnapshot?> ResolveAsync(Guid productId, decimal qty, CancellationToken ct = default);
}
