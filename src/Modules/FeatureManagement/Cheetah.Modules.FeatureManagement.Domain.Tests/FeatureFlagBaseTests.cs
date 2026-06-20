using Cheetah.FeatureManagement;
using Cheetah.Modules.FeatureManagement.Domain.Entities;
using Cheetah.Modules.FeatureManagement.DomainEvents;
using Shouldly;

namespace Cheetah.Modules.FeatureManagement.Domain.Tests;

public class FeatureFlagBaseTests
{
    private static TestFeatureFlag NewFlag() => TestFeatureFlag.Create("deals.kanban-v2", "Kanban v2", "deals");

    [Fact]
    public void Create_is_disabled_and_raises_created_event()
    {
        var flag = TestFeatureFlag.Create("deals.kanban-v2", "Kanban v2", "deals", ownerTeam: "crm-core");

        flag.Id.ShouldNotBe(Guid.Empty);
        flag.Enabled.ShouldBeFalse();
        flag.IsActive.ShouldBeTrue();
        flag.Key.ShouldBe("deals.kanban-v2");
        flag.OwnerTeam.ShouldBe("crm-core"); // расширение наследника работает
        flag.DomainEvents.OfType<FeatureFlagCreatedIntegrationEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public void Enable_disable_are_idempotent_and_raise_toggled()
    {
        var flag = NewFlag();
        flag.ClearDomainEvents();

        flag.Enable();
        flag.Enable(); // повтор — без события
        flag.Enabled.ShouldBeTrue();
        flag.DomainEvents.OfType<FeatureFlagToggledIntegrationEvent>().Count().ShouldBe(1);
        flag.DomainEvents.OfType<FeatureFlagToggledIntegrationEvent>().Single().Enabled.ShouldBeTrue();

        flag.ClearDomainEvents();
        flag.Disable();
        flag.Disable();
        flag.Enabled.ShouldBeFalse();
        flag.DomainEvents.OfType<FeatureFlagToggledIntegrationEvent>().Single().Enabled.ShouldBeFalse();
    }

    [Fact]
    public void SetTargeting_orders_rules_and_raises_changed()
    {
        var flag = NewFlag();
        flag.ClearDomainEvents();

        flag.SetTargeting(new[]
        {
            TargetingRule.Create(flag.Id, 2, "Percentage", "{\"percentage\":50}", null, false),
            TargetingRule.Create(flag.Id, 1, "Roles", "{\"roles\":[\"beta\"]}", null, false)
        });

        flag.Rules.Select(r => r.Order).ShouldBe(new[] { 1, 2 });
        flag.DomainEvents.OfType<FeatureFlagChangedIntegrationEvent>().ShouldHaveSingleItem();
    }

    [Fact]
    public void RegisterMetadata_does_not_reset_enabled_or_targeting()
    {
        var flag = NewFlag();
        flag.Enable();
        flag.SetTargeting(new[] { TargetingRule.Create(flag.Id, 0, "Percentage", "{\"percentage\":10}", null, false) });
        flag.ClearDomainEvents();

        flag.RegisterMetadata("Kanban v2 (renamed)", "new desc", FeatureValueType.Bool);

        flag.Name.ShouldBe("Kanban v2 (renamed)");
        flag.Enabled.ShouldBeTrue();              // не сброшен
        flag.Rules.Count.ShouldBe(1);             // таргетинг сохранён
        flag.DomainEvents.ShouldBeEmpty();        // метаданные не инвалидируют кэш
    }

    [Fact]
    public void SetTenantOverride_replaces_not_duplicates()
    {
        var flag = NewFlag();
        var tenant = Guid.NewGuid();
        flag.ClearDomainEvents();

        flag.SetTenantOverride(TenantOverride.Create(flag.Id, tenant, enabled: true, null));
        flag.SetTenantOverride(TenantOverride.Create(flag.Id, tenant, enabled: false, null));

        flag.Overrides.Count(o => o.TenantId == tenant).ShouldBe(1);
        flag.Overrides.Single(o => o.TenantId == tenant).Enabled.ShouldBeFalse();
        flag.DomainEvents.OfType<FeatureFlagChangedIntegrationEvent>()
            .ShouldAllBe(e => e.TenantId == tenant);
    }

    [Fact]
    public void Initialize_rejects_blank_key()
    {
        Should.Throw<ArgumentException>(() => TestFeatureFlag.Create("  ", "Name", "svc"));
    }
}
