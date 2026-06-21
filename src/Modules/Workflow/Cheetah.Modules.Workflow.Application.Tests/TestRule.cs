using Cheetah.Modules.Workflow.Domain.Entities;
using Cheetah.Modules.Workflow.Shared;

namespace Cheetah.Modules.Workflow.Application.Tests;

/// <summary>Конкретное правило для тестов движка.</summary>
public sealed class TestRule : AutomationRuleBase
{
    private TestRule() { }

    public static TestRule Create(string eventName, string? condition, params (string Type, ActionFailureMode Mode)[] actions)
    {
        var rule = new TestRule();
        rule.InitializeCore(Guid.NewGuid(), "test", "Test", null, null);
        rule.SetTriggers([TriggerBinding.Event(rule.Id, eventName)]);
        rule.SetCondition(condition);
        var ordered = actions.Select((a, i) => RuleAction.Create(rule.Id, i, a.Type, "{}", a.Mode));
        rule.SetActions(ordered);
        rule.Enable();
        rule.ClearDomainEvents();
        return rule;
    }
}
