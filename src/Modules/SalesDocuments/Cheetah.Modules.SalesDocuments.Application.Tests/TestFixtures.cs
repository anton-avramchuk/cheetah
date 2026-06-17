using Cheetah.Modules.SalesDocuments.Application.Abstractions;
using Cheetah.Modules.SalesDocuments.Contracts;
using Cheetah.Modules.SalesDocuments.Domain.Entities;
using Cheetah.Modules.SalesDocuments.Shared;

namespace Cheetah.Modules.SalesDocuments.Application.Tests;

/// <summary>Конкретный наследник документа для тестов прикладного слоя (доп. поле — PaymentTerms).</summary>
public sealed class TestSalesDocument : SalesDocumentBase
{
    public string? PaymentTerms { get; private set; }

    private TestSalesDocument() { }

    public static TestSalesDocument New(
        DocType docType, Guid customerId, Guid ownerId, string currency,
        Guid? dealId, DateTimeOffset? validUntil, string? paymentTerms, Guid? sourceDocumentId = null)
    {
        var doc = new TestSalesDocument();
        doc.InitializeCore(Guid.NewGuid(), docType, customerId, ownerId, currency, dealId, validUntil, sourceDocumentId);
        doc.PaymentTerms = paymentTerms;
        return doc;
    }
}

public sealed record TestCreateDocumentRequest : CreateDocumentRequestBase
{
    public string? PaymentTerms { get; init; }
}

public sealed record TestUpdateDocumentRequest : UpdateDocumentRequestBase;

public sealed record TestSalesDocumentDto : SalesDocumentDtoBase
{
    public string? PaymentTerms { get; init; }
}

public sealed record TestSalesDocumentGridViewModel : SalesDocumentGridViewModelBase;

public sealed class TestSalesDocumentFactory : ISalesDocumentFactory<TestSalesDocument, TestCreateDocumentRequest>
{
    public TestSalesDocument Create(TestCreateDocumentRequest request)
        => TestSalesDocument.New(request.DocType, request.CustomerId, request.OwnerId, request.Currency,
            request.DealId, request.ValidUntil, request.PaymentTerms);

    public TestSalesDocument CreateForConversion(SalesDocumentBase source, DocType toType)
        => TestSalesDocument.New(toType, source.CustomerId, source.OwnerId, source.Currency,
            source.DealId, source.ValidUntil, null, source.Id);
}

public sealed class TestSalesDocumentProjector : ISalesDocumentProjector<TestSalesDocument, TestSalesDocumentDto>
{
    public TestSalesDocumentDto ToDto(TestSalesDocument d) => new()
    {
        Id = d.Id,
        DocType = d.DocType,
        Number = d.Number,
        Status = d.Status,
        CustomerId = d.CustomerId,
        DealId = d.DealId,
        OwnerId = d.OwnerId,
        Currency = d.Currency,
        Subtotal = d.Subtotal,
        DiscountTotal = d.DiscountTotal,
        TaxTotal = d.TaxTotal,
        GrandTotal = d.GrandTotal,
        ValidUntil = d.ValidUntil,
        PdfFileId = d.PdfFileId,
        SourceDocumentId = d.SourceDocumentId,
        PaymentTerms = d.PaymentTerms,
        Lines = d.Lines.Select(l => new SalesDocumentLineDto(
            l.Id, l.ProductId, l.Name, l.UnitPrice, l.Qty, l.DiscountPercent, l.TaxRate, l.LineTotal)).ToArray()
    };
}
