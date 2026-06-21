using Bunit;
using Cheetah.AspNetCore.Blazor.Controls;
using Microsoft.AspNetCore.Components;

namespace Cheetah.AspNetCore.Blazor.Tests.Controls;

public class ContainerControlsTests : BunitContext
{
    private static RenderFragment TwoTabs(bool secondDisabled = false) => builder =>
    {
        builder.OpenComponent<CrmTabPanel>(0);
        builder.AddAttribute(1, nameof(CrmTabPanel.Key), "a");
        builder.AddAttribute(2, nameof(CrmTabPanel.Title), "Первая");
        builder.AddAttribute(3, nameof(CrmTabPanel.ChildContent), RenderFragments.Html("<div class=\"body-a\">AAA</div>"));
        builder.CloseComponent();

        builder.OpenComponent<CrmTabPanel>(4);
        builder.AddAttribute(5, nameof(CrmTabPanel.Key), "b");
        builder.AddAttribute(6, nameof(CrmTabPanel.Title), "Вторая");
        builder.AddAttribute(7, nameof(CrmTabPanel.Disabled), secondDisabled);
        builder.AddAttribute(8, nameof(CrmTabPanel.ChildContent), RenderFragments.Html("<div class=\"body-b\">BBB</div>"));
        builder.CloseComponent();
    };

    [Fact]
    public void Tabs_RenderTabButtons_FirstActive_OnlyActiveContentShown()
    {
        var cut = Render<CrmTabs>(p => p.Add(t => t.ChildContent, TwoTabs()));

        var links = cut.FindAll(".nav-link");
        links.Count.ShouldBe(2);
        links[0].ClassList.ShouldContain("active");
        links[1].ClassList.ShouldNotContain("active");

        cut.Markup.ShouldContain("AAA");
        cut.Markup.ShouldNotContain("BBB");
    }

    [Fact]
    public void Tabs_ClickSecondTab_SwitchesActive_AndRaisesOnTabChanged()
    {
        string? changed = null;
        var cut = Render<CrmTabs>(p => p
            .Add(t => t.ChildContent, TwoTabs())
            .Add(t => t.OnTabChanged, (string k) => changed = k));

        cut.FindAll(".nav-link")[1].Click();

        changed.ShouldBe("b");
        cut.FindAll(".nav-link")[1].ClassList.ShouldContain("active");
        cut.Markup.ShouldContain("BBB");
    }

    [Fact]
    public void Tabs_Pills_UsesNavPills()
    {
        var cut = Render<CrmTabs>(p => p.Add(t => t.Pills, true).Add(t => t.ChildContent, TwoTabs()));
        cut.Find("ul.nav").ClassList.ShouldContain("nav-pills");
    }

    [Fact]
    public void Tabs_DisabledTab_HasDisabledClass()
    {
        var cut = Render<CrmTabs>(p => p.Add(t => t.ChildContent, TwoTabs(secondDisabled: true)));
        cut.FindAll(".nav-link")[1].ClassList.ShouldContain("disabled");
    }

    private static RenderFragment TwoAccordionItems() => builder =>
    {
        builder.OpenComponent<CrmAccordionItem>(0);
        builder.AddAttribute(1, nameof(CrmAccordionItem.Title), "Опыт");
        builder.AddAttribute(2, nameof(CrmAccordionItem.DefaultOpen), true);
        builder.AddAttribute(3, nameof(CrmAccordionItem.ChildContent), RenderFragments.Html("<div class=\"body-1\">B1</div>"));
        builder.CloseComponent();

        builder.OpenComponent<CrmAccordionItem>(4);
        builder.AddAttribute(5, nameof(CrmAccordionItem.Title), "Образование");
        builder.AddAttribute(6, nameof(CrmAccordionItem.ChildContent), RenderFragments.Html("<div class=\"body-2\">B2</div>"));
        builder.CloseComponent();
    };

    [Fact]
    public void Accordion_RendersItems_DefaultOpenExpanded_OthersCollapsed()
    {
        var cut = Render<CrmAccordion>(p => p.Add(a => a.ChildContent, TwoAccordionItems()));

        var headers = cut.FindAll(".accordion-button");
        headers.Count.ShouldBe(2);
        headers[0].ClassList.ShouldNotContain("collapsed"); // открыт по умолчанию
        headers[1].ClassList.ShouldContain("collapsed");

        var collapses = cut.FindAll(".accordion-collapse");
        collapses[0].ClassList.ShouldContain("show");
        collapses[1].ClassList.ShouldNotContain("show");
    }

    [Fact]
    public void Accordion_ClickCollapsedHeader_Expands()
    {
        var cut = Render<CrmAccordion>(p => p.Add(a => a.ChildContent, TwoAccordionItems()));

        cut.FindAll(".accordion-button")[1].Click();

        cut.FindAll(".accordion-button")[1].ClassList.ShouldNotContain("collapsed");
        cut.FindAll(".accordion-collapse")[1].ClassList.ShouldContain("show");
    }
}
