using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Core.StateMachine;
using Cheetah.Modules.SalesDocuments.Application.Documents;
using Cheetah.Modules.SalesDocuments.Application.Exceptions;
using Cheetah.Modules.SalesDocuments.Contracts;
using Cheetah.Modules.SalesDocuments.Domain.Abstractions;
using Cheetah.Modules.SalesDocuments.Domain.Entities;
using Cheetah.Modules.SalesDocuments.DomainEvents;
using Cheetah.Modules.SalesDocuments.Shared;
using Moq;
using Shouldly;

namespace Cheetah.Modules.SalesDocuments.Application.Tests;

public class DocumentHandlerTests
{
    private static readonly Guid Customer = Guid.NewGuid();
    private static readonly Guid Owner = Guid.NewGuid();
    private static readonly Guid Product = Guid.NewGuid();

    private static Mock<IProductPricingPort> Pricing(ProductPriceSnapshot? snapshot = null)
    {
        var mock = new Mock<IProductPricingPort>();
        mock.Setup(p => p.ResolveAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(snapshot);
        return mock;
    }

    private static Mock<IStateMachineValidator<DocumentStatus>> AnyTransition()
    {
        var mock = new Mock<IStateMachineValidator<DocumentStatus>>();
        mock.Setup(s => s.ValidateTransition(It.IsAny<DocumentStatus>(), It.IsAny<DocumentStatus>()));
        return mock;
    }

    [Fact]
    public async Task Create_AppliesOverridePrice_PublishesCreated_ReturnsId()
    {
        var repo = new Mock<IRepository<TestSalesDocument, Guid>>();
        var bus = new Mock<IEventBus>();
        repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreateDocumentCommandHandler<TestSalesDocument, TestCreateDocumentRequest>(
            new TestSalesDocumentFactory(), repo.Object, Pricing().Object, bus.Object);

        var request = new TestCreateDocumentRequest
        {
            DocType = DocType.Quote,
            CustomerId = Customer,
            OwnerId = Owner,
            Currency = "USD",
            Lines = [new AddLineRequest(Product, 2m, UnitPriceOverride: 100m, Name: "Widget", 0m, 0m)]
        };

        var id = await handler.HandleAsync(new CreateDocumentCommand<TestCreateDocumentRequest>(request));

        id.ShouldNotBe(Guid.Empty);
        repo.Verify(r => r.Add(It.IsAny<TestSalesDocument>()), Times.Once);
        bus.Verify(b => b.PublishAsync(It.Is<IEvent>(e => e is DocumentCreatedIntegrationEvent),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddLine_UsesCatalogPrice_WhenNoOverride()
    {
        var doc = TestSalesDocument.New(DocType.Quote, Customer, Owner, "USD", null, null, null);
        var repo = new Mock<IRepository<TestSalesDocument, Guid>>();
        repo.Setup(r => r.GetByIdAsync(doc.Id, It.IsAny<CancellationToken>())).ReturnsAsync(doc);
        repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var bus = new Mock<IEventBus>();

        var handler = new AddLineCommandHandler<TestSalesDocument>(
            repo.Object, Pricing(new ProductPriceSnapshot("Catalog Widget", 50m)).Object, bus.Object);

        await handler.HandleAsync(new AddLineCommand(doc.Id, new AddLineRequest(Product, 3m, null, null, 0m, 0m)));

        doc.Lines.ShouldHaveSingleItem();
        doc.Lines[0].Name.ShouldBe("Catalog Widget");
        doc.Lines[0].UnitPrice.ShouldBe(50m);
        doc.GrandTotal.ShouldBe(150m);
    }

    [Fact]
    public async Task AddLine_NoOverride_NoCatalog_Throws()
    {
        var doc = TestSalesDocument.New(DocType.Quote, Customer, Owner, "USD", null, null, null);
        var repo = new Mock<IRepository<TestSalesDocument, Guid>>();
        repo.Setup(r => r.GetByIdAsync(doc.Id, It.IsAny<CancellationToken>())).ReturnsAsync(doc);

        var handler = new AddLineCommandHandler<TestSalesDocument>(repo.Object, Pricing().Object, new Mock<IEventBus>().Object);

        await Should.ThrowAsync<SalesDocumentsValidationException>(() =>
            handler.HandleAsync(new AddLineCommand(doc.Id, new AddLineRequest(Product, 1m, null, null, 0m, 0m))).AsTask());
    }

    [Fact]
    public async Task Issue_AssignsGeneratedNumber_PublishesSent_ValidatesTransition()
    {
        var doc = TestSalesDocument.New(DocType.Invoice, Customer, Owner, "USD", null, null, null);
        doc.AddLine(Product, "A", 100m, 1m, 0m, 0m);

        var repo = new Mock<IRepository<TestSalesDocument, Guid>>();
        repo.Setup(r => r.GetByIdAsync(doc.Id, It.IsAny<CancellationToken>())).ReturnsAsync(doc);
        repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var numbers = new Mock<IDocumentNumberGenerator>();
        numbers.Setup(n => n.NextAsync(DocType.Invoice, It.IsAny<CancellationToken>())).ReturnsAsync("INV-2026-000001");
        var sm = AnyTransition();
        var bus = new Mock<IEventBus>();

        var handler = new IssueDocumentCommandHandler<TestSalesDocument>(repo.Object, numbers.Object, sm.Object, bus.Object);
        await handler.HandleAsync(new IssueDocumentCommand(doc.Id));

        doc.Number.ShouldBe("INV-2026-000001");
        doc.Status.ShouldBe(DocumentStatus.Issued);
        sm.Verify(s => s.ValidateTransition(DocumentStatus.Draft, DocumentStatus.Issued), Times.Once);
        bus.Verify(b => b.PublishAsync(It.Is<IEvent>(e => e is DocumentSentIntegrationEvent),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AcceptQuote_ValidatesTransition_PublishesAccepted()
    {
        var doc = TestSalesDocument.New(DocType.Quote, Customer, Owner, "USD", null, null, null);
        doc.AddLine(Product, "A", 100m, 1m, 0m, 0m);
        doc.Issue("Q-1");
        doc.ClearDomainEvents();

        var repo = new Mock<IRepository<TestSalesDocument, Guid>>();
        repo.Setup(r => r.GetByIdAsync(doc.Id, It.IsAny<CancellationToken>())).ReturnsAsync(doc);
        repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        var sm = AnyTransition();
        var bus = new Mock<IEventBus>();

        var handler = new AcceptQuoteCommandHandler<TestSalesDocument>(repo.Object, sm.Object, bus.Object);
        await handler.HandleAsync(new AcceptQuoteCommand(doc.Id));

        doc.Status.ShouldBe(DocumentStatus.Accepted);
        sm.Verify(s => s.ValidateTransition(DocumentStatus.Sent, DocumentStatus.Accepted), Times.Once);
        bus.Verify(b => b.PublishAsync(It.Is<IEvent>(e => e is QuoteAcceptedIntegrationEvent),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Convert_CopiesLines_AndSetsSource()
    {
        var quote = TestSalesDocument.New(DocType.Quote, Customer, Owner, "EUR", null, null, null);
        quote.AddLine(Product, "A", 100m, 2m, 0m, 0m);

        var repo = new Mock<IRepository<TestSalesDocument, Guid>>();
        repo.Setup(r => r.GetByIdAsync(quote.Id, It.IsAny<CancellationToken>())).ReturnsAsync(quote);
        repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        TestSalesDocument? added = null;
        repo.Setup(r => r.Add(It.IsAny<TestSalesDocument>())).Callback<TestSalesDocument>(d => added = d);
        var bus = new Mock<IEventBus>();

        var handler = new ConvertDocumentCommandHandler<TestSalesDocument, TestCreateDocumentRequest>(
            repo.Object, new TestSalesDocumentFactory(), bus.Object);

        var newId = await handler.HandleAsync(new ConvertDocumentCommand(quote.Id, DocType.Order));

        added.ShouldNotBeNull();
        added!.Id.ShouldBe(newId);
        added.DocType.ShouldBe(DocType.Order);
        added.Currency.ShouldBe("EUR");
        added.SourceDocumentId.ShouldBe(quote.Id);
        added.Lines.ShouldHaveSingleItem();
        added.GrandTotal.ShouldBe(200m);
    }

    [Fact]
    public async Task GetById_ProjectsToDto()
    {
        var doc = TestSalesDocument.New(DocType.Quote, Customer, Owner, "USD", null, null, "NET30");
        var repo = new Mock<IRepository<TestSalesDocument, Guid>>();
        repo.Setup(r => r.GetByIdAsync(doc.Id, It.IsAny<CancellationToken>())).ReturnsAsync(doc);

        var handler = new GetDocumentByIdQueryHandler<TestSalesDocument, TestSalesDocumentDto>(
            repo.Object, new TestSalesDocumentProjector());

        var dto = await handler.HandleAsync(new GetDocumentByIdQuery<TestSalesDocumentDto>(doc.Id));

        dto.ShouldNotBeNull();
        dto!.PaymentTerms.ShouldBe("NET30");
        dto.CustomerId.ShouldBe(Customer);
    }
}
