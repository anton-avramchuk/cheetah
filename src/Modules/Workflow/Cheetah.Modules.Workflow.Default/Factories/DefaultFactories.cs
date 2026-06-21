using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.Workflow.Application;
using Cheetah.Modules.Workflow.Contracts;
using Cheetah.Modules.Workflow.Default.Contracts;
using Cheetah.Modules.Workflow.Default.Entities;
using Cheetah.Modules.Workflow.Domain.Entities;
using Cheetah.Modules.Workflow.Shared;

namespace Cheetah.Modules.Workflow.Default.Factories;

[Export(LifetimeType.Scoped, typeof(IAutomationRuleFactory<AutomationRule, CreateAutomationRuleRequest>))]
public sealed class RuleFactory : IAutomationRuleFactory<AutomationRule, CreateAutomationRuleRequest>
{
    public AutomationRule Create(CreateAutomationRuleRequest request)
    {
        var rule = AutomationRule.Create(request.Name, request.OwnerService, request.Description, tenantId: null);
        rule.SetCondition(request.ConditionExpression);
        rule.SetTriggers(request.Triggers.Select(t => Map(rule.Id, t)));
        rule.SetActions(request.Actions.Select(a => RuleAction.Create(rule.Id, a.Order, a.ActionType, a.Parameters, a.FailureMode)));
        return rule;
    }

    private static TriggerBinding Map(Guid ruleId, TriggerBindingDto dto) => dto.TriggerType switch
    {
        TriggerType.Schedule => TriggerBinding.Schedule(ruleId, dto.TriggerKey),
        _ => TriggerBinding.Event(ruleId, dto.TriggerKey)
    };
}

[Export(LifetimeType.Scoped, typeof(IAutomationRuleProjector<AutomationRule, AutomationRuleDto>))]
public sealed class RuleProjector : IAutomationRuleProjector<AutomationRule, AutomationRuleDto>
{
    public AutomationRuleDto ToDto(AutomationRule rule) => new()
    {
        Id = rule.Id,
        Name = rule.Name,
        OwnerService = rule.OwnerService,
        Description = rule.Description,
        ConditionExpression = rule.ConditionExpression,
        IsActive = rule.IsActive,
        Triggers = rule.Triggers.Select(t => new TriggerBindingDto(t.TriggerType, t.TriggerKey, t.Parameters)).ToArray(),
        Actions = rule.Actions.Select(a => new RuleActionDto(a.Order, a.ActionType, a.Parameters, a.FailureMode)).ToArray(),
        CreatedAt = rule.CreatedAt,
        UpdatedAt = rule.UpdatedAt
    };
}
