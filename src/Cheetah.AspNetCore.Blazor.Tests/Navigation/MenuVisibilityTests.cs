using System.Security.Claims;
using Cheetah.AspNetCore.Blazor.Abstractions;
using Cheetah.AspNetCore.Blazor.Navigation;
using Cheetah.AspNetCore.Blazor.Navigation.Models;
using Shouldly;

namespace Cheetah.AspNetCore.Blazor.Tests.Navigation;

public class MenuVisibilityTests
{
    private static readonly ClaimsPrincipal User = new(new ClaimsIdentity());

    /// <summary>Разрешает всё — так изолируем проверку фич от проверки прав.</summary>
    private sealed class AllowAll : IMenuAccessEvaluator
    {
        public bool CanSee(ClaimsPrincipal user, string? requiredPermission) => true;
    }

    private sealed class DenyPermission(string denied) : IMenuAccessEvaluator
    {
        public bool CanSee(ClaimsPrincipal user, string? requiredPermission) => requiredPermission != denied;
    }

    private static MenuItem Item(string id, string? feature = null, string? permission = null) =>
        new() { Id = id, Label = id, RequiredFeature = feature, RequiredPermission = permission };

    private static IReadOnlySet<string> Disabled(params string[] features) => new HashSet<string>(features);

    [Fact]
    public void Item_without_feature_is_visible()
        => MenuVisibility.CanSee(Item("a"), User, new AllowAll(), Disabled()).ShouldBeTrue();

    [Fact]
    public void Item_with_enabled_feature_is_visible()
        => MenuVisibility.CanSee(Item("a", feature: "Vacancy.Teams"), User, new AllowAll(), Disabled())
            .ShouldBeTrue();

    [Fact]
    public void Item_with_disabled_feature_is_hidden()
        => MenuVisibility.CanSee(Item("a", feature: "Vacancy.Teams"), User, new AllowAll(), Disabled("Vacancy.Teams"))
            .ShouldBeFalse();

    [Fact]
    public void Permission_and_feature_are_both_required()
    {
        var item = Item("a", feature: "F", permission: "p.view");
        MenuVisibility.CanSee(item, User, new DenyPermission("p.view"), Disabled()).ShouldBeFalse();
        MenuVisibility.CanSee(item, User, new AllowAll(), Disabled("F")).ShouldBeFalse();
        MenuVisibility.CanSee(item, User, new AllowAll(), Disabled()).ShouldBeTrue();
    }

    [Fact]
    public void Group_is_hidden_when_all_children_are_hidden_by_feature()
    {
        var group = Item("group");
        group.Children.Add(Item("child", feature: "F"));

        MenuVisibility.CanSee(group, User, new AllowAll(), Disabled("F")).ShouldBeFalse();
        MenuVisibility.CanSee(group, User, new AllowAll(), Disabled()).ShouldBeTrue();
    }

    [Fact]
    public void Group_with_disabled_feature_hides_children_even_if_they_are_visible()
    {
        var group = Item("group", feature: "F");
        group.Children.Add(Item("child"));

        MenuVisibility.CanSee(group, User, new AllowAll(), Disabled("F")).ShouldBeFalse();
    }

    [Fact]
    public async Task CollectDisabledAsync_asks_evaluator_once_per_distinct_feature()
    {
        var asked = new List<string>();
        var evaluator = new StubEvaluator(asked, enabled: f => f == "On");

        var menu = new Menu("main");
        var section = new MenuSection { Id = "main" };
        section.Items.Add(Item("a", feature: "On"));
        section.Items.Add(Item("b", feature: "Off"));
        section.Items.Add(Item("c", feature: "Off")); // тот же ключ — второй раз не спрашиваем
        var group = Item("g", feature: "On");
        group.Children.Add(Item("d", feature: "Off"));
        section.Items.Add(group);
        section.Items.Add(Item("e")); // без фичи — не спрашиваем вовсе
        menu.Sections.Add(section);

        var disabled = await MenuVisibility.CollectDisabledAsync(menu, evaluator);

        disabled.ShouldBe(new HashSet<string> { "Off" }, ignoreOrder: true);
        asked.Order().ShouldBe(["Off", "On"]);
    }

    private sealed class StubEvaluator(List<string> asked, Func<string, bool> enabled) : IFeatureVisibilityEvaluator
    {
        public ValueTask<bool> IsEnabledAsync(string feature, CancellationToken ct = default)
        {
            asked.Add(feature);
            return ValueTask.FromResult(enabled(feature));
        }
    }
}
