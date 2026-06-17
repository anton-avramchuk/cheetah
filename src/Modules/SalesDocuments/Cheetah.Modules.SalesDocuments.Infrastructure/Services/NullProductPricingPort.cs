using Cheetah.Modules.SalesDocuments.Domain.Abstractions;

namespace Cheetah.Modules.SalesDocuments.Infrastructure.Services;

/// <summary>
/// Дефолтный «пустой» адаптер каталога: всегда возвращает <c>null</c>. Регистрируется, чтобы DI
/// разрешал <see cref="IProductPricingPort"/> до подключения реального адаптера над <c>Catalog.Client</c>.
/// С ним строки добавляются только с явной ценой (<c>UnitPriceOverride</c>) — см. план §7.2 (follow-up).
/// </summary>
public sealed class NullProductPricingPort : IProductPricingPort
{
    public ValueTask<ProductPriceSnapshot?> ResolveAsync(Guid productId, decimal qty, CancellationToken ct = default)
        => ValueTask.FromResult<ProductPriceSnapshot?>(null);
}
