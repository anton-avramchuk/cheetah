using Bunit;
using Cheetah.AspNetCore.Blazor.Controls;

namespace Cheetah.AspNetCore.Blazor.Tests.Controls;

public class DisplayControlsTests : BunitContext
{
    // ---- CrmButton ----------------------------------------------------------

    [Fact]
    public void Button_RendersVariantAndChildContent()
    {
        var cut = Render<CrmButton>(p => p.Add(b => b.Variant, "danger").AddChildContent("Удалить"));

        var button = cut.Find("button");
        button.ClassList.ShouldContain("btn");
        button.ClassList.ShouldContain("btn-danger");
        cut.Markup.ShouldContain("Удалить");
    }

    [Fact]
    public void Button_Outline_UsesOutlineVariantClass()
    {
        var cut = Render<CrmButton>(p => p.Add(b => b.Variant, "success").Add(b => b.Outline, true));
        cut.Find("button").ClassList.ShouldContain("btn-outline-success");
    }

    [Fact]
    public void Button_SmallSize_AddsSizeClass()
    {
        var cut = Render<CrmButton>(p => p.Add(b => b.Size, "sm"));
        cut.Find("button").ClassList.ShouldContain("btn-sm");
    }

    [Fact]
    public void Button_Loading_ShowsSpinner_AndIsDisabled()
    {
        var cut = Render<CrmButton>(p => p.Add(b => b.Loading, true));

        cut.Markup.ShouldContain("spinner-border");
        cut.Find("button").HasAttribute("disabled").ShouldBeTrue();
    }

    [Fact]
    public void Button_Click_InvokesOnClick()
    {
        var clicked = false;
        var cut = Render<CrmButton>(p => p.Add(b => b.OnClick, () => clicked = true));

        cut.Find("button").Click();

        clicked.ShouldBeTrue();
    }

    [Fact]
    public void Button_Disabled_DoesNotInvokeOnClick()
    {
        var clicked = false;
        var cut = Render<CrmButton>(p => p.Add(b => b.Disabled, true).Add(b => b.OnClick, () => clicked = true));

        cut.Find("button").Click();

        clicked.ShouldBeFalse();
    }

    // ---- CrmBadge -----------------------------------------------------------

    [Fact]
    public void Badge_RendersVariantAndContent()
    {
        var cut = Render<CrmBadge>(p => p.Add(b => b.Variant, "success").AddChildContent("Активен"));

        var span = cut.Find("span.badge");
        span.ClassList.ShouldContain("text-bg-success");
        span.TextContent.ShouldContain("Активен");
    }

    [Fact]
    public void Badge_Pill_AddsRoundedPill()
    {
        var cut = Render<CrmBadge>(p => p.Add(b => b.Pill, true).AddChildContent("x"));
        cut.Find("span.badge").ClassList.ShouldContain("rounded-pill");
    }

    // ---- CrmAlert -----------------------------------------------------------

    [Fact]
    public void Alert_DefaultVariantInfo_RendersMessage()
    {
        var cut = Render<CrmAlert>(p => p.Add(a => a.Message, "Внимание"));

        var alert = cut.Find(".alert");
        alert.ClassList.ShouldContain("alert-info");
        alert.TextContent.ShouldContain("Внимание");
    }

    [Fact]
    public void Alert_WithTitle_RendersHeading()
    {
        var cut = Render<CrmAlert>(p => p.Add(a => a.Variant, "warning").Add(a => a.Title, "Заголовок").Add(a => a.Message, "m"));

        cut.Find(".alert").ClassList.ShouldContain("alert-warning");
        cut.Find("h6.alert-heading").TextContent.ShouldBe("Заголовок");
    }

    [Fact]
    public void Alert_Dismiss_HidesAlert_AndRaisesCallback()
    {
        var dismissed = false;
        var cut = Render<CrmAlert>(p => p
            .Add(a => a.Message, "m")
            .Add(a => a.Dismissible, true)
            .Add(a => a.OnDismissed, () => dismissed = true));

        cut.Find("button.btn-close").Click();

        dismissed.ShouldBeTrue();
        cut.Markup.ShouldNotContain("alert");
    }

    // ---- CrmSpinner ---------------------------------------------------------

    [Fact]
    public void Spinner_Visible_RendersSpinnerBorder()
    {
        var cut = Render<CrmSpinner>(p => p.Add(s => s.Variant, "secondary"));

        cut.Markup.ShouldContain("spinner-border");
        cut.Markup.ShouldContain("text-secondary");
    }

    [Fact]
    public void Spinner_NotVisible_RendersNothing()
    {
        var cut = Render<CrmSpinner>(p => p.Add(s => s.Visible, false));
        cut.Markup.Trim().ShouldBeEmpty();
    }

    [Fact]
    public void Spinner_Grow_UsesGrowVariant()
    {
        var cut = Render<CrmSpinner>(p => p.Add(s => s.Grow, true));
        cut.Markup.ShouldContain("spinner-grow");
    }

    [Fact]
    public void Spinner_Overlay_RendersFixedOverlay()
    {
        var cut = Render<CrmSpinner>(p => p.Add(s => s.Overlay, true).Add(s => s.Text, "Загрузка"));
        cut.Markup.ShouldContain("position-fixed");
        cut.Markup.ShouldContain("Загрузка");
    }

    // ---- CrmProgressBar -----------------------------------------------------

    [Fact]
    public void ProgressBar_RendersWidthAndAria()
    {
        var cut = Render<CrmProgressBar>(p => p.Add(b => b.Value, 75).Add(b => b.Variant, "success").Add(b => b.Striped, true));

        var bar = cut.Find(".progress-bar");
        bar.ClassList.ShouldContain("bg-success");
        bar.ClassList.ShouldContain("progress-bar-striped");
        bar.GetAttribute("style").ShouldContain("width: 75%");
        bar.GetAttribute("aria-valuenow").ShouldBe("75");
    }

    [Fact]
    public void ProgressBar_ShowValue_RendersPercentInHeader()
    {
        var cut = Render<CrmProgressBar>(p => p.Add(b => b.Value, 42).Add(b => b.Label, "L").Add(b => b.ShowValue, true));
        cut.Markup.ShouldContain("42%");
    }

    // ---- CrmCard ------------------------------------------------------------

    [Fact]
    public void Card_RendersTitleSubtitleAndBody()
    {
        var cut = Render<CrmCard>(p => p
            .Add(c => c.Title, "Карточка")
            .Add(c => c.Subtitle, "Подзаголовок")
            .AddChildContent("<p>Тело</p>"));

        cut.Find(".card-title").TextContent.ShouldBe("Карточка");
        cut.Find(".card-subtitle").TextContent.ShouldContain("Подзаголовок");
        cut.Find(".card-body").TextContent.ShouldContain("Тело");
    }

    [Fact]
    public void Card_RendersHeaderAndFooterSlots()
    {
        var cut = Render<CrmCard>(p => p
            .Add(c => c.HeaderContent, RenderFragments.Html("<span class=\"hdr\">H</span>"))
            .Add(c => c.FooterContent, RenderFragments.Html("<span class=\"ftr\">F</span>"))
            .AddChildContent("body"));

        cut.Find(".card-header .hdr").TextContent.ShouldBe("H");
        cut.Find(".card-footer .ftr").TextContent.ShouldBe("F");
    }

    // ---- CrmTooltip ---------------------------------------------------------

    [Fact]
    public void Tooltip_RendersBootstrapDataAttributes()
    {
        var cut = Render<CrmTooltip>(p => p
            .Add(t => t.Text, "Подсказка")
            .Add(t => t.Placement, "bottom")
            .AddChildContent("<button>X</button>"));

        var span = cut.Find("span[data-bs-toggle=tooltip]");
        span.GetAttribute("title").ShouldBe("Подсказка");
        span.GetAttribute("data-bs-placement").ShouldBe("bottom");
        cut.Markup.ShouldContain("<button>X</button>");
    }
}
