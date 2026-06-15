using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Core.Specification;
using Cheetah.Core.StateMachine;
using Cheetah.Modules.Leads.Application.Exceptions;
using Cheetah.Modules.Leads.Application.Leads;
using Cheetah.Modules.Leads.Domain.Abstractions;
using Cheetah.Modules.Leads.DomainEvents;
using Cheetah.Modules.Leads.Shared;
using Moq;
using Shouldly;

namespace Cheetah.Modules.Leads.Application.Tests;

public class LeadCommandHandlerTests
{
    private readonly Mock<IRepository<TestLead, Guid>> _repo = new();
    private readonly Mock<IEventBus> _eventBus = new();
    private readonly Mock<IStateMachineValidator<LeadStatus>> _sm = new();

    [Fact]
    public async Task Create_AddsSavesAndPublishesCreatedEvent()
    {
        var handler = new CreateLeadCommandHandler<TestLead, TestCreateRequest>(
            new TestLeadFactory(), _repo.Object, _eventBus.Object);

        var id = await handler.HandleAsync(new CreateLeadCommand<TestCreateRequest>(TestData.CreateRequest()));

        id.ShouldNotBe(Guid.Empty);
        _repo.Verify(r => r.Add(It.Is<TestLead>(l => l.FullName == "John Doe")), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _eventBus.Verify(b => b.PublishAsync(
            It.Is<IEvent>(e => e is LeadCreatedIntegrationEvent), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_PassesExtensionField()
    {
        var handler = new CreateLeadCommandHandler<TestLead, TestCreateRequest>(
            new TestLeadFactory(), _repo.Object, _eventBus.Object);

        var req = TestData.CreateRequest() with { Industry = "IT" };
        await handler.HandleAsync(new CreateLeadCommand<TestCreateRequest>(req));

        _repo.Verify(r => r.Add(It.Is<TestLead>(l => l.Industry == "IT")), Times.Once);
    }

    [Fact]
    public async Task Create_DuplicateEmail_Throws()
    {
        _repo.Setup(r => r.ExistsAsync(It.IsAny<ISpecification<TestLead>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var handler = new CreateLeadCommandHandler<TestLead, TestCreateRequest>(
            new TestLeadFactory(), _repo.Object, _eventBus.Object);

        await Should.ThrowAsync<LeadValidationException>(() =>
            handler.HandleAsync(new CreateLeadCommand<TestCreateRequest>(
                TestData.CreateRequest(email: "dup@x.io"))).AsTask());

        _repo.Verify(r => r.Add(It.IsAny<TestLead>()), Times.Never);
    }

    [Fact]
    public async Task Qualify_ValidatesTransition_AndPublishes()
    {
        var lead = TestData.NewLead();
        _repo.Setup(r => r.GetByIdAsync(lead.Id, It.IsAny<CancellationToken>())).ReturnsAsync(lead);
        var handler = new QualifyLeadCommandHandler<TestLead>(_repo.Object, _eventBus.Object, _sm.Object);

        await handler.HandleAsync(new QualifyLeadCommand(lead.Id));

        _sm.Verify(s => s.ValidateTransition(LeadStatus.New, LeadStatus.Qualified), Times.Once);
        lead.Status.ShouldBe(LeadStatus.Qualified);
        _eventBus.Verify(b => b.PublishAsync(
            It.Is<IEvent>(e => e is LeadQualifiedIntegrationEvent), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Disqualify_SetsStatus_AndPublishes()
    {
        var lead = TestData.NewLead();
        _repo.Setup(r => r.GetByIdAsync(lead.Id, It.IsAny<CancellationToken>())).ReturnsAsync(lead);
        var handler = new DisqualifyLeadCommandHandler<TestLead>(_repo.Object, _eventBus.Object, _sm.Object);

        await handler.HandleAsync(new DisqualifyLeadCommand(lead.Id, "no budget"));

        lead.Status.ShouldBe(LeadStatus.Disqualified);
        _eventBus.Verify(b => b.PublishAsync(
            It.Is<IEvent>(e => e is LeadDisqualifiedIntegrationEvent), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_NotFound_Throws()
    {
        _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TestLead?)null);
        var handler = new UpdateLeadCommandHandler<TestLead, TestUpdateRequest>(_repo.Object);

        await Should.ThrowAsync<LeadValidationException>(() =>
            handler.HandleAsync(new UpdateLeadCommand<TestUpdateRequest>(
                Guid.NewGuid(), new TestUpdateRequest { FullName = "X" })).AsTask());
    }

    [Fact]
    public async Task Convert_CallsOrchestrator_MarksConverted_AndPublishes()
    {
        var lead = TestData.QualifiedLead();
        var customerId = Guid.NewGuid();
        var dealId = Guid.NewGuid();
        _repo.Setup(r => r.GetByIdAsync(lead.Id, It.IsAny<CancellationToken>())).ReturnsAsync(lead);
        var orchestrator = new Mock<ILeadConversionOrchestrator>();
        orchestrator.Setup(o => o.ConvertAsync(It.IsAny<LeadConversionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConversionOutcome(customerId, dealId));
        var handler = new ConvertLeadCommandHandler<TestLead, TestConvertRequest>(
            _repo.Object, orchestrator.Object, _eventBus.Object);

        var result = await handler.HandleAsync(new ConvertLeadCommand<TestConvertRequest>(
            lead.Id, new TestConvertRequest { CreateDeal = true, DealTitle = "Deal", PipelineId = Guid.NewGuid() }));

        result.CustomerId.ShouldBe(customerId);
        result.DealId.ShouldBe(dealId);
        lead.Status.ShouldBe(LeadStatus.Converted);
        lead.ConvertedCustomerId.ShouldBe(customerId);
        orchestrator.Verify(o => o.ConvertAsync(
            It.Is<LeadConversionRequest>(r => r.LeadId == lead.Id && r.CreateDeal), It.IsAny<CancellationToken>()), Times.Once);
        _eventBus.Verify(b => b.PublishAsync(
            It.Is<IEvent>(e => e is LeadConvertedIntegrationEvent), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Convert_NotQualified_Throws()
    {
        var lead = TestData.NewLead(); // New, not Qualified
        _repo.Setup(r => r.GetByIdAsync(lead.Id, It.IsAny<CancellationToken>())).ReturnsAsync(lead);
        var orchestrator = new Mock<ILeadConversionOrchestrator>();
        var handler = new ConvertLeadCommandHandler<TestLead, TestConvertRequest>(
            _repo.Object, orchestrator.Object, _eventBus.Object);

        await Should.ThrowAsync<LeadValidationException>(() =>
            handler.HandleAsync(new ConvertLeadCommand<TestConvertRequest>(lead.Id, new TestConvertRequest())).AsTask());

        orchestrator.Verify(o => o.ConvertAsync(
            It.IsAny<LeadConversionRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
