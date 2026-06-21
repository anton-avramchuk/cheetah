using Cheetah.Modules.Workflow.Domain.Entities;
using Cheetah.Modules.Workflow.DomainEvents;
using Cheetah.Modules.Workflow.Shared;
using Shouldly;

namespace Cheetah.Modules.Workflow.Domain.Tests;

public class AutomationRuleBaseTests
{
    private static TestAutomationRule Executable()
    {
        var rule = TestAutomationRule.Create("Deal won → task", "Deals", ownerTeam: "crm-core");
        rule.SetTriggers([TriggerBinding.Event(rule.Id, "DealWonIntegrationEvent")]);
        rule.SetActions([RuleAction.Create(rule.Id, 0, BuiltInActions.CreateActivity, "{}")]);
        return rule;
    }

    [Fact]
    public void Create_is_disabled_and_raises_created_event()
    {
        var rule = TestAutomationRule.Create("rule", "Deals", ownerTeam: "crm-core");

        rule.Id.ShouldNotBe(Guid.Empty);
        rule.IsActive.ShouldBeFalse();
        rule.OwnerTeam.ShouldBe("crm-core"); // расширение наследника работает
        rule.DomainEvents.OfType<AutomationRuleCreatedIntegrationEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public void Enable_on_empty_rule_throws()
    {
        var rule = TestAutomationRule.Create("rule", "Deals");
        Should.Throw<InvalidOperationException>(() => rule.Enable());
    }

    [Fact]
    public void Enable_disable_are_idempotent_and_raise_events()
    {
        var rule = Executable();
        rule.ClearDomainEvents();

        rule.Enable();
        rule.Enable(); // повторно — не дублирует
        rule.IsActive.ShouldBeTrue();
        rule.DomainEvents.OfType<AutomationRuleEnabledIntegrationEvent>().ShouldHaveSingleItem();

        rule.ClearDomainEvents();
        rule.Disable();
        rule.Disable();
        rule.IsActive.ShouldBeFalse();
        rule.DomainEvents.OfType<AutomationRuleDisabledIntegrationEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public void SetActions_orders_by_order()
    {
        var rule = TestAutomationRule.Create("rule", "Deals");
        rule.SetActions([
            RuleAction.Create(rule.Id, 2, "C", "{}"),
            RuleAction.Create(rule.Id, 0, "A", "{}"),
            RuleAction.Create(rule.Id, 1, "B", "{}")
        ]);

        rule.Actions.Select(a => a.ActionType).ShouldBe(["A", "B", "C"]);
    }

    [Fact]
    public void SetTriggers_raises_changed_and_exposes_event_keys()
    {
        var rule = TestAutomationRule.Create("rule", "Deals");
        rule.ClearDomainEvents();

        rule.SetTriggers([
            TriggerBinding.Event(rule.Id, "DealWonIntegrationEvent"),
            TriggerBinding.Schedule(rule.Id, "0 9 * * 1")
        ]);

        rule.DomainEvents.OfType<AutomationRuleChangedIntegrationEvent>().ShouldHaveSingleItem();
        rule.EventTriggerKeys().ShouldBe(["DealWonIntegrationEvent"]); // только Event-триггеры
    }
}
