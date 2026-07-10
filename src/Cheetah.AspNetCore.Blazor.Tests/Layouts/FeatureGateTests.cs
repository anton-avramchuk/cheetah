using Bunit;
using Cheetah.AspNetCore.Blazor.Abstractions;
using Cheetah.AspNetCore.Blazor.Layouts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Cheetah.AspNetCore.Blazor.Tests.Layouts;

public class FeatureGateTests : BunitContext
{
    [RequireFeature("Vacancy.Teams")]
    private sealed class GatedPage : ComponentBase;

    private sealed class OpenPage : ComponentBase;

    private sealed class BasePage : ComponentBase;

    [RequireFeature("Inherited.Feature")]
    private class GatedBase : ComponentBase;

    private sealed class DerivedPage : GatedBase;

    private sealed class StubEvaluator(Func<string, bool> enabled) : IFeatureVisibilityEvaluator
    {
        public ValueTask<bool> IsEnabledAsync(string feature, CancellationToken ct = default)
            => ValueTask.FromResult(enabled(feature));
    }

    private static RouteData Route<TPage>() where TPage : IComponent
        => new(typeof(TPage), new Dictionary<string, object?>());

    private IRenderedComponent<FeatureGate> RenderGate(RouteData route, bool enabled) =>
        Render<FeatureGate>(p => p
            .Add(g => g.RouteData, route)
            .Add(g => g.ChildContent, (RenderFragment)(b => b.AddMarkupContent(0, "<span>PAGE-BODY</span>")))
            .Add(g => g.Disabled, (RenderFragment)(b => b.AddMarkupContent(0, "<span>FEATURE-OFF</span>"))));

    private void UseEvaluator(bool enabled) =>
        Services.AddSingleton<IFeatureVisibilityEvaluator>(new StubEvaluator(_ => enabled));

    // ---- ResolveFeature ----------------------------------------------------

    [Fact]
    public void ResolveFeature_returns_null_for_page_without_attribute()
        => FeatureGate.ResolveFeature(typeof(OpenPage)).ShouldBeNull();

    [Fact]
    public void ResolveFeature_reads_attribute()
        => FeatureGate.ResolveFeature(typeof(GatedPage)).ShouldBe("Vacancy.Teams");

    [Fact]
    public void ResolveFeature_is_inherited_from_base_page()
        => FeatureGate.ResolveFeature(typeof(DerivedPage)).ShouldBe("Inherited.Feature");

    [Fact]
    public void Attribute_rejects_empty_feature()
        => Should.Throw<ArgumentException>(() => new RequireFeatureAttribute("  "));

    // ---- Рендер ------------------------------------------------------------

    [Fact]
    public void Page_without_attribute_renders_even_when_everything_is_disabled()
    {
        UseEvaluator(enabled: false);

        RenderGate(Route<OpenPage>(), enabled: false).Markup.ShouldContain("PAGE-BODY");
    }

    [Fact]
    public void Gated_page_renders_when_feature_is_enabled()
    {
        UseEvaluator(enabled: true);

        RenderGate(Route<GatedPage>(), enabled: true).Markup.ShouldContain("PAGE-BODY");
    }

    [Fact]
    public void Gated_page_renders_disabled_fragment_when_feature_is_off()
    {
        UseEvaluator(enabled: false);

        var markup = RenderGate(Route<GatedPage>(), enabled: false).Markup;

        markup.ShouldContain("FEATURE-OFF");
        markup.ShouldNotContain("PAGE-BODY"); // страница не должна мелькнуть
    }

    [Fact]
    public void Disabled_page_falls_back_to_not_found_when_no_fragment_given()
    {
        UseEvaluator(enabled: false);

        var cut = Render<FeatureGate>(p => p
            .Add(g => g.RouteData, Route<GatedPage>())
            .Add(g => g.ChildContent, (RenderFragment)(b => b.AddMarkupContent(0, "<span>PAGE-BODY</span>"))));

        cut.Markup.ShouldNotContain("PAGE-BODY");
        cut.FindComponents<NotFoundPage>().Count.ShouldBe(1);
    }
}
