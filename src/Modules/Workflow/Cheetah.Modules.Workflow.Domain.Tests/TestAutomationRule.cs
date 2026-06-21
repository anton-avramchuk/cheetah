using Cheetah.Modules.Workflow.Domain.Entities;

namespace Cheetah.Modules.Workflow.Domain.Tests;

/// <summary>Конкретный наследник <see cref="AutomationRuleBase"/> с доп. полем — проверяет структурную
/// расширяемость (как TestFeatureFlag в FeatureManagement).</summary>
public sealed class TestAutomationRule : AutomationRuleBase
{
    public string? OwnerTeam { get; private set; }

    private TestAutomationRule() { }

    public static TestAutomationRule Create(string name, string ownerService,
        string? description = null, string? ownerTeam = null, Guid? tenantId = null)
    {
        var rule = new TestAutomationRule();
        rule.InitializeCore(Guid.NewGuid(), name, ownerService, description, tenantId);
        rule.OwnerTeam = ownerTeam;
        return rule;
    }
}
