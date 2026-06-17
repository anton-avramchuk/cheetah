using Cheetah.Core.DependencyInjection;
using Cheetah.Mapping.Mapster;
using Cheetah.Modules.SalesDocuments.Application.Documents;
using Cheetah.Modules.SalesDocuments.Contracts;
using Mapster;

namespace Cheetah.Modules.SalesDocuments.Api.Mapping;

/// <summary>
/// Mapster-маппинги модуля документов: Request операций → конкретные команды. Маппинги расширяемого
/// документа (Request → CreateDocumentCommand&lt;…&gt;, Document → ViewModel) объявляет наследник/хост.
/// </summary>
[Export(LifetimeType.Singleton, typeof(IMapsterMappingProfile))]
public sealed class SalesDocumentsMappingProfile : IMapsterMappingProfile
{
    public void Configure(TypeAdapterConfig config)
    {
        config.NewConfig<AddLineToDocumentRequest, AddLineCommand>()
            .Map(d => d.DocumentId, s => s.Id)
            .Map(d => d.Line, s => new AddLineRequest(
                s.ProductId, s.Qty, s.UnitPriceOverride, s.Name, s.DiscountPercent, s.TaxRate));

        config.NewConfig<RemoveLineRequest, RemoveLineCommand>()
            .Map(d => d.DocumentId, s => s.Id);

        config.NewConfig<IssueDocumentRequest, IssueDocumentCommand>()
            .Map(d => d.DocumentId, s => s.Id);

        config.NewConfig<AcceptQuoteRequest, AcceptQuoteCommand>()
            .Map(d => d.DocumentId, s => s.Id);

        config.NewConfig<RejectQuoteRequest, RejectQuoteCommand>()
            .Map(d => d.DocumentId, s => s.Id);

        config.NewConfig<MarkInvoicePaidRequest, MarkInvoicePaidCommand>()
            .Map(d => d.DocumentId, s => s.Id);

        config.NewConfig<CancelDocumentRequest, CancelDocumentCommand>()
            .Map(d => d.DocumentId, s => s.Id);

        config.NewConfig<ConvertDocumentRequest, ConvertDocumentCommand>()
            .Map(d => d.DocumentId, s => s.Id);

        config.NewConfig<GenerateDocumentPdfRequest, GenerateDocumentPdfCommand>()
            .Map(d => d.DocumentId, s => s.Id);
    }
}
