using Cheetah.Modules.Deals.Domain.Entities;
using Cheetah.Modules.Deals.DomainEvents;
using Cheetah.Modules.Deals.Shared;
using Shouldly;

namespace Cheetah.Modules.Deals.Domain.Tests;

public class DealTests
{
    private static Pipeline NewPipeline()
    {
        var p = Pipeline.Create("Sales", isDefault: true);
        p.AddStage("Qualified", 1, 10, StageType.Open);
        p.AddStage("Proposal", 2, 50, StageType.Open);
        p.AddStage("Won", 3, 100, StageType.Won);
        p.AddStage("Lost", 4, 0, StageType.Lost);
        return p;
    }

    private static Deal NewDeal(Pipeline p, decimal amount = 1000m)
        => Deal.Create("Big deal", p, new Money(amount, "USD"), Guid.NewGuid(), Guid.NewGuid());

    [Fact]
    public void Create_StartsOnFirstOpenStage_AndRaisesCreatedEvent()
    {
        var p = NewPipeline();
        var deal = NewDeal(p);

        deal.Status.ShouldBe(DealStatus.Open);
        deal.StageId.ShouldBe(p.FirstStage().Id);
        deal.DomainEvents.OfType<DealCreatedIntegrationEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public void MoveToStage_OpenStage_RaisesStageChanged()
    {
        var p = NewPipeline();
        var deal = NewDeal(p);
        var proposal = p.Stages.First(s => s.Name == "Proposal");

        deal.MoveToStage(proposal, Guid.NewGuid());

        deal.StageId.ShouldBe(proposal.Id);
        deal.History.ShouldHaveSingleItem();
        deal.DomainEvents.OfType<DealStageChangedIntegrationEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public void MoveToStage_WonStage_TerminalizesAndRaisesWon()
    {
        var p = NewPipeline();
        var deal = NewDeal(p);
        var won = p.Stages.First(s => s.Type == StageType.Won);

        deal.MoveToStage(won, Guid.NewGuid());

        deal.Status.ShouldBe(DealStatus.Won);
        deal.ClosedAt.ShouldNotBeNull();
        deal.DomainEvents.OfType<DealWonIntegrationEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public void MoveToStage_LostStage_RequiresLoseMethod()
    {
        var p = NewPipeline();
        var deal = NewDeal(p);
        var lost = p.Stages.First(s => s.Type == StageType.Lost);

        Should.Throw<InvalidOperationException>(() => deal.MoveToStage(lost, Guid.NewGuid()));
    }

    [Fact]
    public void MoveToStage_OnClosedDeal_Throws()
    {
        var p = NewPipeline();
        var deal = NewDeal(p);
        deal.Win(Guid.NewGuid());
        var proposal = p.Stages.First(s => s.Name == "Proposal");

        Should.Throw<InvalidOperationException>(() => deal.MoveToStage(proposal, Guid.NewGuid()));
    }

    [Fact]
    public void Win_WithZeroAmount_Throws()
    {
        var p = NewPipeline();
        var deal = NewDeal(p, amount: 0m);

        Should.Throw<InvalidOperationException>(() => deal.Win(Guid.NewGuid()));
    }

    [Fact]
    public void Lose_WithoutReason_Throws()
        => Should.Throw<ArgumentException>(() => NewDeal(NewPipeline()).Lose("  ", Guid.NewGuid()));

    [Fact]
    public void Lose_SetsReasonAndClosedAt_RaisesLost()
    {
        var deal = NewDeal(NewPipeline());
        deal.Lose("Budget", Guid.NewGuid());

        deal.Status.ShouldBe(DealStatus.Lost);
        deal.LostReason.ShouldBe("Budget");
        deal.ClosedAt.ShouldNotBeNull();
        deal.DomainEvents.OfType<DealLostIntegrationEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public void Reopen_ClearsClosureAndLostReason()
    {
        var p = NewPipeline();
        var deal = NewDeal(p);
        deal.Lose("Budget", Guid.NewGuid());

        deal.Reopen(p.FirstStage().Id);

        deal.Status.ShouldBe(DealStatus.Open);
        deal.ClosedAt.ShouldBeNull();
        deal.LostReason.ShouldBeNull();
    }

    [Fact]
    public void AssignOwner_DifferentOwner_RaisesOwnerChanged()
    {
        var deal = NewDeal(NewPipeline());
        deal.AssignOwner(Guid.NewGuid());
        deal.DomainEvents.OfType<DealOwnerChangedIntegrationEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public void AssignOwner_SameOwner_NoEvent()
    {
        var deal = NewDeal(NewPipeline());
        deal.AssignOwner(deal.OwnerId);
        deal.DomainEvents.OfType<DealOwnerChangedIntegrationEvent>().ShouldBeEmpty();
    }
}
