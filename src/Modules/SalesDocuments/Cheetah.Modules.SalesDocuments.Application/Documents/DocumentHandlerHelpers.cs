using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Modules.SalesDocuments.Application.Exceptions;
using Cheetah.Modules.SalesDocuments.Contracts;
using Cheetah.Modules.SalesDocuments.Domain.Abstractions;
using Cheetah.Modules.SalesDocuments.Domain.Entities;

namespace Cheetah.Modules.SalesDocuments.Application.Documents;

/// <summary>Общие хвосты хендлеров: сохранить+опубликовать события, загрузить документ, разрешить строку.</summary>
internal static class DocumentHandlerHelpers
{
    public static async ValueTask SaveAndPublishAsync<TDoc>(
        IRepository<TDoc, Guid> repository, IEventBus eventBus, TDoc document, CancellationToken ct)
        where TDoc : SalesDocumentBase
    {
        await repository.SaveChangesAsync(ct);
        foreach (var e in document.DomainEvents)
            await eventBus.PublishAsync(e, ct);
        document.ClearDomainEvents();
    }

    public static async ValueTask<TDoc> GetOrThrowAsync<TDoc>(
        IRepository<TDoc, Guid> repository, Guid id, CancellationToken ct)
        where TDoc : SalesDocumentBase
        => await repository.GetByIdAsync(id, ct)
            ?? throw new SalesDocumentsValidationException($"Document '{id}' not found");

    /// <summary>
    /// Применяет строки запроса к документу через доменный <c>AddLine</c>, разрешая снимок цены и
    /// наименования: явный <c>UnitPriceOverride</c>/<c>Name</c> приоритетнее, иначе — из каталога.
    /// </summary>
    public static async ValueTask ApplyLinesAsync<TDoc>(
        TDoc document, IEnumerable<AddLineRequest> lines, IProductPricingPort pricing, CancellationToken ct)
        where TDoc : SalesDocumentBase
    {
        foreach (var line in lines)
            await AddResolvedLineAsync(document, line, pricing, ct);
    }

    public static async ValueTask AddResolvedLineAsync<TDoc>(
        TDoc document, AddLineRequest line, IProductPricingPort pricing, CancellationToken ct)
        where TDoc : SalesDocumentBase
    {
        ProductPriceSnapshot? snapshot = null;
        if (line.UnitPriceOverride is null || line.Name is null)
            snapshot = await pricing.ResolveAsync(line.ProductId, line.Qty, ct);

        var unitPrice = line.UnitPriceOverride
            ?? snapshot?.UnitPrice
            ?? throw new SalesDocumentsValidationException(
                $"Unit price for product '{line.ProductId}' is unavailable (no override and no catalog price).");

        var name = line.Name
            ?? snapshot?.Name
            ?? throw new SalesDocumentsValidationException(
                $"Name for product '{line.ProductId}' is unavailable (no override and no catalog product).");

        document.AddLine(line.ProductId, name, unitPrice, line.Qty, line.DiscountPercent, line.TaxRate);
    }
}
