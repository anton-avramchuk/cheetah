using Cheetah.Modules.SalesDocuments.Domain.Entities;
using Cheetah.Modules.SalesDocuments.Shared;

namespace Cheetah.Modules.SalesDocuments.Domain.Tests;

/// <summary>
/// Конкретный наследник <see cref="SalesDocumentBase"/> для проверки базового поведения. Доп. поле
/// <see cref="PaymentTerms"/> демонстрирует расширяемость агрегата.
/// </summary>
public sealed class TestSalesDocument : SalesDocumentBase
{
    public string? PaymentTerms { get; private set; }

    private TestSalesDocument() { }

    public static TestSalesDocument Create(
        DocType docType, Guid customerId, Guid ownerId, string currency = "USD",
        Guid? dealId = null, DateTimeOffset? validUntil = null, string? paymentTerms = null)
    {
        var doc = new TestSalesDocument();
        doc.InitializeCore(Guid.NewGuid(), docType, customerId, ownerId, currency, dealId, validUntil);
        doc.PaymentTerms = paymentTerms;
        return doc;
    }

    public static TestSalesDocument CreateForConversion(SalesDocumentBase source, DocType toType)
    {
        var doc = new TestSalesDocument();
        doc.InitializeCore(Guid.NewGuid(), toType, source.CustomerId, source.OwnerId, source.Currency,
            source.DealId, source.ValidUntil, source.Id);
        return doc;
    }
}
