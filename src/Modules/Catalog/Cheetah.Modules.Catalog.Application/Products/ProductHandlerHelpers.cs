using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Modules.Catalog.Domain.Entities;

namespace Cheetah.Modules.Catalog.Application.Products;

/// <summary>Общий хвост хендлеров мутации товара: сохранить и опубликовать доменные события.</summary>
internal static class ProductHandlerHelpers
{
    public static async ValueTask SaveAndPublishAsync<TProduct>(
        IRepository<TProduct, Guid> repository, IEventBus eventBus, TProduct product, CancellationToken ct)
        where TProduct : ProductBase
    {
        await repository.SaveChangesAsync(ct);
        foreach (var e in product.DomainEvents)
            await eventBus.PublishAsync(e, ct);
        product.ClearDomainEvents();
    }
}
