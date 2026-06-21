using Cheetah.Core.Domain;
using Cheetah.Modules.Workflow.Shared;

namespace Cheetah.Modules.Workflow.Domain.Entities;

/// <summary>Действие правила: операция (<see cref="ActionType"/>) с параметрами-шаблоном и режимом сбоя.</summary>
public sealed class RuleAction : Entity<Guid>
{
    public Guid RuleId { get; private set; }
    public int Order { get; private set; }
    public string ActionType { get; private set; } = null!;
    public string Parameters { get; private set; } = "{}";
    public ActionFailureMode FailureMode { get; private set; }

    private RuleAction() { }

    public static RuleAction Create(Guid ruleId, int order, string actionType, string? parametersJson,
        ActionFailureMode failureMode = ActionFailureMode.StopRule)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(actionType);
        return new RuleAction
        {
            Id = Guid.NewGuid(), RuleId = ruleId, Order = order,
            ActionType = actionType.Trim(),
            Parameters = string.IsNullOrWhiteSpace(parametersJson) ? "{}" : parametersJson,
            FailureMode = failureMode
        };
    }
}
