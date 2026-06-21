using Cheetah.Core.Domain;
using Cheetah.Modules.Workflow.DomainEvents;
using Cheetah.Modules.Workflow.Shared;
using Cheetah.Workflow;

namespace Cheetah.Modules.Workflow.Domain.Entities;

/// <summary>
/// Журнал одного срабатывания правила. Уникален по <c>(RuleId, EventId)</c> — это одновременно аудит и
/// механизм дедупа (повторная доставка того же события не запускает правило дважды).
/// </summary>
public sealed class AutomationRun : AggregateRoot<Guid>, ICreateAtEntity
{
    private readonly List<AutomationRunStep> _steps = new();

    public Guid RuleId { get; private set; }
    public Guid EventId { get; private set; }
    public string EventName { get; private set; } = null!;
    public RunStatus Status { get; private set; }
    public string? PayloadSnapshot { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public IReadOnlyList<AutomationRunStep> Steps => _steps;
    public DateTimeOffset? CreatedAt { get; set; }

    private AutomationRun() { }

    public static AutomationRun Start(Guid ruleId, WorkflowEventEnvelope trigger, string payloadJson)
        => new()
        {
            Id = Guid.NewGuid(),
            RuleId = ruleId,
            EventId = trigger.SourceEventId,
            EventName = trigger.EventName,
            Status = RunStatus.Pending,
            PayloadSnapshot = payloadJson
        };

    public void RecordSuccess(string actionType)
        => _steps.Add(AutomationRunStep.Success(Id, _steps.Count, actionType));

    public void RecordFailure(string actionType, string error)
        => _steps.Add(AutomationRunStep.Failure(Id, _steps.Count, actionType, error));

    public void Complete()
    {
        var anyFailed = _steps.Any(s => s.Status == RunStatus.Failed);
        var anySucceeded = _steps.Any(s => s.Status == RunStatus.Succeeded);
        Status = (anyFailed, anySucceeded) switch
        {
            (true, true) => RunStatus.PartiallyFailed,
            (true, false) => RunStatus.Failed,
            _ => RunStatus.Succeeded
        };
        CompletedAt = DateTimeOffset.UtcNow;

        if (Status == RunStatus.Failed)
            AddDomainEvent(new AutomationRunFailedIntegrationEvent(Id, RuleId, _steps.LastOrDefault()?.Error ?? "unknown"));
        else
            AddDomainEvent(new AutomationRunCompletedIntegrationEvent(Id, RuleId, Status.ToString()));
    }
}

/// <summary>Результат исполнения одного действия в рамках срабатывания правила.</summary>
public sealed class AutomationRunStep : Entity<Guid>
{
    public Guid RunId { get; private set; }
    public int Order { get; private set; }
    public string ActionType { get; private set; } = null!;
    public RunStatus Status { get; private set; }
    public string? Error { get; private set; }

    private AutomationRunStep() { }

    internal static AutomationRunStep Success(Guid runId, int order, string actionType)
        => new() { Id = Guid.NewGuid(), RunId = runId, Order = order, ActionType = actionType, Status = RunStatus.Succeeded };

    internal static AutomationRunStep Failure(Guid runId, int order, string actionType, string error)
        => new() { Id = Guid.NewGuid(), RunId = runId, Order = order, ActionType = actionType, Status = RunStatus.Failed, Error = error };
}
