using Cheetah.Modules.Workflow.Domain.Entities;
using Cheetah.Modules.Workflow.DomainEvents;
using Cheetah.Modules.Workflow.Shared;
using Cheetah.Workflow;
using Shouldly;

namespace Cheetah.Modules.Workflow.Domain.Tests;

public class AutomationRunTests
{
    private static WorkflowEventEnvelope Envelope()
        => new("DealWonIntegrationEvent", Guid.NewGuid(),
               new Dictionary<string, object?> { ["DealId"] = Guid.NewGuid() });

    private static AutomationRun NewRun() => AutomationRun.Start(Guid.NewGuid(), Envelope(), "{}");

    [Fact]
    public void All_success_completes_as_succeeded()
    {
        var run = NewRun();
        run.RecordSuccess("A");
        run.RecordSuccess("B");
        run.Complete();

        run.Status.ShouldBe(RunStatus.Succeeded);
        run.CompletedAt.ShouldNotBeNull();
        run.DomainEvents.OfType<AutomationRunCompletedIntegrationEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public void All_failed_completes_as_failed_and_raises_failed_event()
    {
        var run = NewRun();
        run.RecordFailure("A", "boom");
        run.Complete();

        run.Status.ShouldBe(RunStatus.Failed);
        run.DomainEvents.OfType<AutomationRunFailedIntegrationEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public void Mixed_completes_as_partially_failed()
    {
        var run = NewRun();
        run.RecordSuccess("A");
        run.RecordFailure("B", "boom");
        run.Complete();

        run.Status.ShouldBe(RunStatus.PartiallyFailed);
        run.DomainEvents.OfType<AutomationRunCompletedIntegrationEvent>().ShouldHaveSingleItem();
        run.Steps.Count.ShouldBe(2);
    }

    [Fact]
    public void Start_carries_source_event_id_for_dedup()
    {
        var envelope = Envelope();
        var run = AutomationRun.Start(Guid.NewGuid(), envelope, "{}");
        run.EventId.ShouldBe(envelope.SourceEventId);
        run.EventName.ShouldBe("DealWonIntegrationEvent");
    }
}
