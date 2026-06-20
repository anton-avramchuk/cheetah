using Cheetah.FeatureManagement.Filters;
using Shouldly;

namespace Cheetah.FeatureManagement.Tests;

public class FeatureManagerTests
{
    private static FeatureManager Manager(FeatureDefinition def, params IFeatureFilter[] filters)
        => new(new FakeDefinitionProvider(def), filters);

    [Fact]
    public async Task Unregistered_flag_is_disabled()
    {
        var mgr = new FeatureManager(new FakeDefinitionProvider(), []);
        (await mgr.IsEnabledAsync("nope")).ShouldBeFalse();
    }

    [Fact]
    public async Task KillSwitch_off_bypasses_targeting()
    {
        var def = new FeatureDefinition("f", Enabled: false, FeatureValueType.Bool,
            [new TargetingRuleDefinition(0, "Always", new Dictionary<string, object?>())], []);
        var mgr = Manager(def, new StubFilter("Always", true));

        (await mgr.IsEnabledAsync("f")).ShouldBeFalse();
    }

    [Fact]
    public async Task Enabled_without_rules_is_on()
    {
        var def = new FeatureDefinition("f", Enabled: true, FeatureValueType.Bool, [], []);
        var mgr = Manager(def);

        (await mgr.IsEnabledAsync("f")).ShouldBeTrue();
    }

    [Fact]
    public async Task Enabled_with_rules_but_none_match_is_off()
    {
        var def = new FeatureDefinition("f", Enabled: true, FeatureValueType.Bool,
            [new TargetingRuleDefinition(0, "Never", new Dictionary<string, object?>())], []);
        var mgr = Manager(def, new StubFilter("Never", false));

        (await mgr.IsEnabledAsync("f")).ShouldBeFalse();
    }

    [Fact]
    public async Task Deny_rule_short_circuits_to_off_even_if_later_rule_allows()
    {
        var def = new FeatureDefinition("f", Enabled: true, FeatureValueType.Bool,
        [
            new TargetingRuleDefinition(0, "Deny", new Dictionary<string, object?>(), Negate: true),
            new TargetingRuleDefinition(1, "Allow", new Dictionary<string, object?>())
        ], []);
        var mgr = Manager(def, new StubFilter("Deny", true), new StubFilter("Allow", true));

        (await mgr.IsEnabledAsync("f")).ShouldBeFalse();
    }

    [Fact]
    public async Task Unknown_filter_is_skipped_failsafe()
    {
        var def = new FeatureDefinition("f", Enabled: true, FeatureValueType.Bool,
        [
            new TargetingRuleDefinition(0, "DoesNotExist", new Dictionary<string, object?>()),
            new TargetingRuleDefinition(1, "Allow", new Dictionary<string, object?>())
        ], []);
        var mgr = Manager(def, new StubFilter("Allow", true));

        (await mgr.IsEnabledAsync("f")).ShouldBeTrue();
    }

    [Fact]
    public async Task Percentage_rollout_is_stable_for_same_user()
    {
        var def = new FeatureDefinition("rollout", Enabled: true, FeatureValueType.Bool,
            [new TargetingRuleDefinition(0, BuiltInFilterNames.Percentage,
                new Dictionary<string, object?> { ["percentage"] = 50 })], []);
        var mgr = Manager(def, new PercentageFeatureFilter());

        var user = new FeatureContext { UserId = Guid.NewGuid() };
        var first = await mgr.IsEnabledAsync("rollout", user);
        for (var i = 0; i < 20; i++)
            (await mgr.IsEnabledAsync("rollout", user)).ShouldBe(first);
    }

    [Fact]
    public async Task Percentage_zero_off_hundred_on()
    {
        var user = new FeatureContext { UserId = Guid.NewGuid() };

        var off = Manager(new FeatureDefinition("z", true, FeatureValueType.Bool,
            [new TargetingRuleDefinition(0, BuiltInFilterNames.Percentage,
                new Dictionary<string, object?> { ["percentage"] = 0 })], []), new PercentageFeatureFilter());
        (await off.IsEnabledAsync("z", user)).ShouldBeFalse();

        var on = Manager(new FeatureDefinition("h", true, FeatureValueType.Bool,
            [new TargetingRuleDefinition(0, BuiltInFilterNames.Percentage,
                new Dictionary<string, object?> { ["percentage"] = 100 })], []), new PercentageFeatureFilter());
        (await on.IsEnabledAsync("h", user)).ShouldBeTrue();
    }

    [Fact]
    public async Task Roughly_half_of_users_enabled_at_50_percent()
    {
        var def = new FeatureDefinition("r", true, FeatureValueType.Bool,
            [new TargetingRuleDefinition(0, BuiltInFilterNames.Percentage,
                new Dictionary<string, object?> { ["percentage"] = 50 })], []);
        var mgr = Manager(def, new PercentageFeatureFilter());

        var enabled = 0;
        const int total = 2000;
        for (var i = 0; i < total; i++)
            if (await mgr.IsEnabledAsync("r", new FeatureContext { UserId = Guid.NewGuid() }))
                enabled++;

        // допускаем разброс ±7%
        (enabled / (double)total).ShouldBeInRange(0.43, 0.57);
    }

    [Fact]
    public async Task Variant_flag_returns_weighted_variant_when_enabled()
    {
        var def = new FeatureDefinition("exp", true, FeatureValueType.Variant, [],
        [
            new VariantDefinition("control", "0", 50),
            new VariantDefinition("treatment", "1", 50)
        ]);
        var mgr = Manager(def);

        var v = await mgr.GetVariantAsync("exp", new FeatureContext { UserId = Guid.NewGuid() });
        v.ShouldNotBeNull();
        v!.Name.ShouldBeOneOf("control", "treatment");
    }

    [Fact]
    public async Task Variant_is_null_when_flag_disabled()
    {
        var def = new FeatureDefinition("exp", Enabled: false, FeatureValueType.Variant, [],
            [new VariantDefinition("control", "0", 1)]);
        var mgr = Manager(def);

        (await mgr.GetVariantAsync("exp")).ShouldBeNull();
    }

    [Fact]
    public async Task Rule_result_variant_wins_over_weights()
    {
        var def = new FeatureDefinition("exp", true, FeatureValueType.Variant,
            [new TargetingRuleDefinition(0, "Allow", new Dictionary<string, object?>(), ResultVariant: "treatment")],
            [new VariantDefinition("control", "0", 99), new VariantDefinition("treatment", "1", 1)]);
        var mgr = Manager(def, new StubFilter("Allow", true));

        var v = await mgr.GetVariantAsync("exp", new FeatureContext { UserId = Guid.NewGuid() });
        v!.Name.ShouldBe("treatment");
    }
}
