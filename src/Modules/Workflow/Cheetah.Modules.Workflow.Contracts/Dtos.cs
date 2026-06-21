using Cheetah.Contracts.Responses;
using Cheetah.Modules.Workflow.Shared;

namespace Cheetah.Modules.Workflow.Contracts;

/// <summary>Расширяемая ViewModel правила. Наследник добавляет поля через <c>init</c>-свойства.</summary>
public abstract record AutomationRuleDtoBase : ICrmResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string OwnerService { get; init; } = null!;
    public string? Description { get; init; }
    public string? ConditionExpression { get; init; }
    public bool IsActive { get; init; }
    public IReadOnlyList<TriggerBindingDto> Triggers { get; init; } = [];
    public IReadOnlyList<RuleActionDto> Actions { get; init; } = [];
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}

public abstract record CreateAutomationRuleRequestBase
{
    public string Name { get; init; } = null!;
    public string OwnerService { get; init; } = null!;
    public string? Description { get; init; }
    public string? ConditionExpression { get; init; }
    public IReadOnlyList<TriggerBindingDto> Triggers { get; init; } = [];
    public IReadOnlyList<RuleActionDto> Actions { get; init; } = [];
}

public abstract record UpdateAutomationRuleRequestBase : CreateAutomationRuleRequestBase;

public sealed record TriggerBindingDto(TriggerType TriggerType, string TriggerKey, string? Parameters);

public sealed record RuleActionDto(int Order, string ActionType, string Parameters, ActionFailureMode FailureMode);

public sealed record AutomationRunDto(
    Guid Id, Guid RuleId, Guid EventId, string EventName, RunStatus Status,
    IReadOnlyList<AutomationRunStepDto> Steps, DateTimeOffset OccurredAt);

public sealed record AutomationRunStepDto(int Order, string ActionType, RunStatus Status, string? Error);

/// <summary>Результат dry-run «теста» правила: сработало ли условие и какой план действий.</summary>
public sealed record TestRunResult(bool ConditionMatched, IReadOnlyList<PlannedActionDto> PlannedActions);

public sealed record PlannedActionDto(int Order, string ActionType, IReadOnlyDictionary<string, object?> RenderedParameters);
