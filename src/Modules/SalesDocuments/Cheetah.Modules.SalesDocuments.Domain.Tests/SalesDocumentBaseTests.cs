using Cheetah.Modules.SalesDocuments.DomainEvents;
using Cheetah.Modules.SalesDocuments.Shared;
using Shouldly;

namespace Cheetah.Modules.SalesDocuments.Domain.Tests;

public class SalesDocumentBaseTests
{
    private static readonly Guid Customer = Guid.NewGuid();
    private static readonly Guid Owner = Guid.NewGuid();
    private static readonly Guid Product = Guid.NewGuid();

    [Fact]
    public void Create_ProducesDraft_WithCreatedEvent()
    {
        var doc = TestSalesDocument.Create(DocType.Quote, Customer, Owner);

        doc.Status.ShouldBe(DocumentStatus.Draft);
        doc.Currency.ShouldBe("USD");
        doc.GrandTotal.ShouldBe(0m);
        doc.DomainEvents.OfType<DocumentCreatedIntegrationEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public void AddLine_RecalculatesTotals()
    {
        var doc = TestSalesDocument.Create(DocType.Quote, Customer, Owner);

        // 2 шт по 100, скидка 10% (=20), налог 20% от 180 (=36)
        doc.AddLine(Product, "Widget", unitPrice: 100m, qty: 2m, discountPercent: 10m, taxRate: 20m);

        doc.Subtotal.ShouldBe(200m);
        doc.DiscountTotal.ShouldBe(20m);
        doc.TaxTotal.ShouldBe(36m);
        doc.GrandTotal.ShouldBe(216m);
        doc.Lines.ShouldHaveSingleItem();
    }

    [Fact]
    public void AddLine_TwoLines_SumsTotals()
    {
        var doc = TestSalesDocument.Create(DocType.Invoice, Customer, Owner);
        doc.AddLine(Product, "A", 50m, 1m, 0m, 0m);
        doc.AddLine(Guid.NewGuid(), "B", 10m, 3m, 0m, 0m);

        doc.Subtotal.ShouldBe(80m);
        doc.GrandTotal.ShouldBe(80m);
    }

    [Fact]
    public void RemoveLine_RecalculatesTotals()
    {
        var doc = TestSalesDocument.Create(DocType.Quote, Customer, Owner);
        doc.AddLine(Product, "A", 100m, 1m, 0m, 0m);
        var lineId = doc.Lines[0].Id;

        doc.RemoveLine(lineId);

        doc.Lines.ShouldBeEmpty();
        doc.GrandTotal.ShouldBe(0m);
    }

    [Fact]
    public void Issue_WithoutLines_Throws()
    {
        var doc = TestSalesDocument.Create(DocType.Quote, Customer, Owner);
        Should.Throw<InvalidOperationException>(() => doc.Issue("Q-1"));
    }

    [Theory]
    [InlineData(DocType.Quote, DocumentStatus.Sent)]
    [InlineData(DocType.Order, DocumentStatus.Confirmed)]
    [InlineData(DocType.Invoice, DocumentStatus.Issued)]
    public void Issue_SetsStatusByDocType_AndAssignsNumber(DocType type, DocumentStatus expected)
    {
        var doc = TestSalesDocument.Create(type, Customer, Owner);
        doc.AddLine(Product, "A", 100m, 1m, 0m, 0m);

        doc.Issue("DOC-2026-1");

        doc.Status.ShouldBe(expected);
        doc.Number.ShouldBe("DOC-2026-1");
        doc.DomainEvents.OfType<DocumentSentIntegrationEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public void AddLine_AfterIssue_Throws()
    {
        var doc = TestSalesDocument.Create(DocType.Quote, Customer, Owner);
        doc.AddLine(Product, "A", 100m, 1m, 0m, 0m);
        doc.Issue("Q-1");

        Should.Throw<InvalidOperationException>(() => doc.AddLine(Product, "B", 1m, 1m, 0m, 0m));
    }

    [Fact]
    public void Accept_OnNonQuote_Throws()
    {
        var doc = TestSalesDocument.Create(DocType.Invoice, Customer, Owner);
        Should.Throw<InvalidOperationException>(() => doc.Accept());
    }

    [Fact]
    public void Accept_OnQuote_RaisesEvent()
    {
        var doc = TestSalesDocument.Create(DocType.Quote, Customer, Owner);
        doc.AddLine(Product, "A", 100m, 1m, 0m, 0m);
        doc.Issue("Q-1");
        doc.ClearDomainEvents();

        doc.Accept();

        doc.Status.ShouldBe(DocumentStatus.Accepted);
        doc.DomainEvents.OfType<QuoteAcceptedIntegrationEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public void MarkPaid_OnNonInvoice_Throws()
    {
        var doc = TestSalesDocument.Create(DocType.Quote, Customer, Owner);
        Should.Throw<InvalidOperationException>(() => doc.MarkPaid());
    }

    [Fact]
    public void Cancel_AfterPaid_Throws()
    {
        var doc = TestSalesDocument.Create(DocType.Invoice, Customer, Owner);
        doc.AddLine(Product, "A", 100m, 1m, 0m, 0m);
        doc.Issue("INV-1");
        doc.MarkPaid();

        Should.Throw<InvalidOperationException>(() => doc.Cancel("nope"));
    }

    [Fact]
    public void Conversion_CopiesHeader_AndLinesViaAddLine()
    {
        var quote = TestSalesDocument.Create(DocType.Quote, Customer, Owner, "EUR");
        quote.AddLine(Product, "A", 100m, 2m, 0m, 0m);

        var order = TestSalesDocument.CreateForConversion(quote, DocType.Order);
        foreach (var l in quote.Lines)
            order.AddLine(l.ProductId, l.Name, l.UnitPrice, l.Qty, l.DiscountPercent, l.TaxRate);

        order.DocType.ShouldBe(DocType.Order);
        order.Currency.ShouldBe("EUR");
        order.SourceDocumentId.ShouldBe(quote.Id);
        order.GrandTotal.ShouldBe(200m);
        order.Lines.Count.ShouldBe(1);
    }

    [Fact]
    public void MarkOverdue_OnlyIssuedInvoice()
    {
        var doc = TestSalesDocument.Create(DocType.Invoice, Customer, Owner);
        doc.AddLine(Product, "A", 100m, 1m, 0m, 0m);
        doc.Issue("INV-1");

        doc.MarkOverdue();

        doc.Status.ShouldBe(DocumentStatus.Overdue);
        doc.DomainEvents.OfType<InvoiceOverdueIntegrationEvent>().ShouldHaveSingleItem();
    }
}
