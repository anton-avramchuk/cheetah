using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.Events;
using Cheetah.Core.Specification;
using Cheetah.Expressions;
using Cheetah.Modules.Workflow.Domain.Entities;
using Cheetah.Modules.Workflow.Shared;
using Cheetah.Workflow;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;

namespace Cheetah.Modules.Workflow.Application.Tests;

public class WorkflowEngineTests
{
    private readonly Mock<IRuleMatcher<TestRule>> _matcher = new();
    private readonly Mock<IExpressionEvaluator> _expressions = new();
    private readonly Mock<IWorkflowActionExecutor> _executor = new();
    private readonly Mock<IRepository<AutomationRun, Guid>> _runs = new();
    private readonly Mock<IEventBus> _bus = new();
    private readonly List<string> _executed = new();

    private WorkflowEngine<TestRule> BuildEngine()
    {
        _executor
            .Setup(e => e.ExecuteAsync(It.IsAny<string>(), It.IsAny<WorkflowActionContext>(), It.IsAny<CancellationToken>()))
            .Returns((string type, WorkflowActionContext _, CancellationToken _) =>
            {
                _executed.Add(type);
                return ValueTask.CompletedTask;
            });

        return new WorkflowEngine<TestRule>(
            _matcher.Object, _expressions.Object, _executor.Object,
            new ParameterRenderer(), _runs.Object, _bus.Object,
            NullLogger<WorkflowEngine<TestRule>>.Instance);
    }

    private static WorkflowEventEnvelope Envelope() =>
        new("DealWonIntegrationEvent", Guid.NewGuid(),
            new Dictionary<string, object?> { ["Amount"] = 150000L });

    private void SetupRules(params TestRule[] rules) =>
        _matcher.Setup(m => m.MatchAsync(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(rules);

    private void SetupCondition(bool result) =>
        _expressions.Setup(e => e.EvaluateBooleanAsync(It.IsAny<string>(),
                It.IsAny<IReadOnlyDictionary<string, object?>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

    [Fact]
    public async Task No_rules_does_nothing()
    {
        SetupRules();
        await BuildEngine().HandleTriggerAsync(Envelope(), CancellationToken.None);
        _executor.VerifyNoOtherCalls();
        _runs.Verify(r => r.Add(It.IsAny<AutomationRun>()), Times.Never);
    }

    [Fact]
    public async Task Condition_false_skips_rule()
    {
        SetupRules(TestRule.Create("DealWonIntegrationEvent", "{\"==\":[1,2]}", ("A", ActionFailureMode.StopRule)));
        SetupCondition(false);

        await BuildEngine().HandleTriggerAsync(Envelope(), CancellationToken.None);

        _executed.ShouldBeEmpty();
        _runs.Verify(r => r.Add(It.IsAny<AutomationRun>()), Times.Never);
    }

    [Fact]
    public async Task Null_condition_runs_actions_in_order()
    {
        SetupRules(TestRule.Create("DealWonIntegrationEvent", null,
            ("A", ActionFailureMode.ContinueNext),
            ("B", ActionFailureMode.ContinueNext),
            ("C", ActionFailureMode.ContinueNext)));

        await BuildEngine().HandleTriggerAsync(Envelope(), CancellationToken.None);

        _executed.ShouldBe(["A", "B", "C"]);
        _runs.Verify(r => r.Add(It.IsAny<AutomationRun>()), Times.Once);
    }

    [Fact]
    public async Task StopRule_failure_halts_subsequent_actions()
    {
        SetupRules(TestRule.Create("DealWonIntegrationEvent", null,
            ("A", ActionFailureMode.ContinueNext),
            ("B", ActionFailureMode.StopRule),
            ("C", ActionFailureMode.ContinueNext)));
        var engine = BuildEngine();
        // специфичный override ПОСЛЕ BuildEngine — иначе общий setup перекрыл бы его (last-match-wins).
        _executor.Setup(e => e.ExecuteAsync("B", It.IsAny<WorkflowActionContext>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("boom"));

        await engine.HandleTriggerAsync(Envelope(), CancellationToken.None);

        _executed.ShouldBe(["A"]); // C не исполнено — правило прервано на B
    }

    [Fact]
    public async Task ContinueNext_failure_keeps_going()
    {
        SetupRules(TestRule.Create("DealWonIntegrationEvent", null,
            ("A", ActionFailureMode.ContinueNext),
            ("B", ActionFailureMode.ContinueNext),
            ("C", ActionFailureMode.ContinueNext)));
        var engine = BuildEngine();
        _executor.Setup(e => e.ExecuteAsync("B", It.IsAny<WorkflowActionContext>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("boom"));

        await engine.HandleTriggerAsync(Envelope(), CancellationToken.None);

        _executed.ShouldBe(["A", "C"]); // B упало, но C исполнено
    }

    [Fact]
    public async Task Dedup_skips_when_run_already_exists()
    {
        SetupRules(TestRule.Create("DealWonIntegrationEvent", null, ("A", ActionFailureMode.StopRule)));
        _runs.Setup(r => r.ExistsAsync(It.IsAny<ISpecification<AutomationRun>>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(true); // уже отрабатывало это событие

        await BuildEngine().HandleTriggerAsync(Envelope(), CancellationToken.None);

        _executed.ShouldBeEmpty();
        _runs.Verify(r => r.Add(It.IsAny<AutomationRun>()), Times.Never);
    }
}
