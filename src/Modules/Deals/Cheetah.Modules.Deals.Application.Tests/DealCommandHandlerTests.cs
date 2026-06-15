using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Core.StateMachine;
using Cheetah.Modules.Deals.Application.Deals;
using Cheetah.Modules.Deals.Domain.Abstractions;
using Cheetah.Modules.Deals.Domain.Entities;
using Cheetah.Modules.Deals.DomainEvents;
using Cheetah.Modules.Deals.Shared;
using Moq;
using Shouldly;

namespace Cheetah.Modules.Deals.Application.Tests;

public class DealCommandHandlerTests
{
    private static Pipeline NewPipeline()
    {
        var p = Pipeline.Create("Sales", isDefault: true);
        p.AddStage("Qualified", 1, 10, StageType.Open);
        p.AddStage("Proposal", 2, 50, StageType.Open);
        p.AddStage("Won", 3, 100, StageType.Won);
        return p;
    }

    private static Deal NewDeal(Pipeline p, decimal amount = 1000m)
        => Deal.Create("Big deal", p, new Money(amount, "USD"), Guid.NewGuid(), Guid.NewGuid());

    [Fact]
    public async Task CreateDeal_PublishesEventsBeforeSave_AndReturnsId()
    {
        var pipeline = NewPipeline();
        var deals = new Mock<IRepository<Deal, Guid>>();
        var pipelines = new Mock<IPipelineRepository>();
        var bus = new Mock<IEventBus>();

        pipelines.Setup(x => x.GetWithStagesAsync(pipeline.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pipeline);

        var saveCalled = false;
        var publishedBeforeSave = false;
        bus.Setup(b => b.PublishAsync(It.IsAny<IEvent>(), It.IsAny<CancellationToken>()))
            .Callback(() => { if (!saveCalled) publishedBeforeSave = true; })
            .Returns(ValueTask.CompletedTask);
        deals.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => saveCalled = true)
            .ReturnsAsync(1);

        var handler = new CreateDealCommandHandler(deals.Object, pipelines.Object, bus.Object);
        var cmd = new CreateDealCommand("Big deal", pipeline.Id, 1000m, "USD",
            Guid.NewGuid(), Guid.NewGuid(), null, null);

        var id = await handler.HandleAsync(cmd, CancellationToken.None);

        id.ShouldNotBe(Guid.Empty);
        deals.Verify(d => d.Add(It.IsAny<Deal>()), Times.Once);
        deals.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        bus.Verify(b => b.PublishAsync(It.Is<IEvent>(e => e is DealCreatedIntegrationEvent), It.IsAny<CancellationToken>()), Times.Once);
        publishedBeforeSave.ShouldBeTrue();
    }

    [Fact]
    public async Task ChangeDealStage_MovesToOpenStage_PublishesStageChanged()
    {
        var pipeline = NewPipeline();
        var deal = NewDeal(pipeline);
        var proposal = pipeline.Stages.First(s => s.Name == "Proposal");

        var deals = new Mock<IRepository<Deal, Guid>>();
        var pipelines = new Mock<IPipelineRepository>();
        var bus = new Mock<IEventBus>();

        deals.Setup(d => d.GetByIdAsync(deal.Id, It.IsAny<CancellationToken>())).ReturnsAsync(deal);
        pipelines.Setup(p => p.GetWithStagesAsync(pipeline.Id, It.IsAny<CancellationToken>())).ReturnsAsync(pipeline);
        deals.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        bus.Setup(b => b.PublishAsync(It.IsAny<IEvent>(), It.IsAny<CancellationToken>())).Returns(ValueTask.CompletedTask);

        var handler = new ChangeDealStageCommandHandler(deals.Object, pipelines.Object, bus.Object);
        await handler.HandleAsync(new ChangeDealStageCommand(deal.Id, proposal.Id, Guid.NewGuid()), CancellationToken.None);

        deal.StageId.ShouldBe(proposal.Id);
        bus.Verify(b => b.PublishAsync(It.Is<IEvent>(e => e is DealStageChangedIntegrationEvent), It.IsAny<CancellationToken>()), Times.Once);
        deals.Verify(d => d.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task WinDeal_ValidatesTransition_AndPublishesWon()
    {
        var pipeline = NewPipeline();
        var deal = NewDeal(pipeline);

        var deals = new Mock<IRepository<Deal, Guid>>();
        var bus = new Mock<IEventBus>();
        var sm = new Mock<IStateMachineValidator<DealStatus>>();

        deals.Setup(d => d.GetByIdAsync(deal.Id, It.IsAny<CancellationToken>())).ReturnsAsync(deal);
        deals.Setup(d => d.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        bus.Setup(b => b.PublishAsync(It.IsAny<IEvent>(), It.IsAny<CancellationToken>())).Returns(ValueTask.CompletedTask);

        var handler = new WinDealCommandHandler(deals.Object, bus.Object, sm.Object);
        await handler.HandleAsync(new WinDealCommand(deal.Id, Guid.NewGuid()), CancellationToken.None);

        sm.Verify(s => s.ValidateTransition(DealStatus.Open, DealStatus.Won), Times.Once);
        deal.Status.ShouldBe(DealStatus.Won);
        bus.Verify(b => b.PublishAsync(It.Is<IEvent>(e => e is DealWonIntegrationEvent), It.IsAny<CancellationToken>()), Times.Once);
    }
}
