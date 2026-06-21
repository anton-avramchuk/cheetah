using Cheetah.Modules.Workflow.Domain.Entities;

namespace Cheetah.Modules.Workflow.Default.Entities;

/// <summary>Конкретное правило «из коробки» (без доп. полей). Наследник приложения может объявить своё.</summary>
public sealed class AutomationRule : AutomationRuleBase
{
    private AutomationRule() { }

    public static AutomationRule Create(string name, string ownerService, string? description, Guid? tenantId)
    {
        var rule = new AutomationRule();
        rule.InitializeCore(Guid.NewGuid(), name, ownerService, description, tenantId);
        return rule;
    }
}
