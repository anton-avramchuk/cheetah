using Bunit;
using Cheetah.AspNetCore.Blazor.Controls;

namespace Cheetah.AspNetCore.Blazor.Tests.Controls;

public class RedesignControlsTests : BunitContext
{
    // ---- CrmStatusBadge -----------------------------------------------------

    [Fact]
    public void StatusBadge_RendersVariantClass_Text_AndDot()
    {
        var cut = Render<CrmStatusBadge>(p => p.Add(b => b.Variant, "success").Add(b => b.Text, "Активен"));

        var span = cut.Find("span.crm-status");
        span.ClassList.ShouldContain("crm-status-success");
        span.TextContent.ShouldContain("Активен");
        cut.FindAll(".crm-status-dot").Count.ShouldBe(1);
    }

    [Fact]
    public void StatusBadge_DotFalse_OmitsDot()
    {
        var cut = Render<CrmStatusBadge>(p => p.Add(b => b.Variant, "danger").Add(b => b.Dot, false).Add(b => b.Text, "x"));
        cut.FindAll(".crm-status-dot").Count.ShouldBe(0);
    }

    [Fact]
    public void StatusBadge_ChildContent_OverridesText()
    {
        var cut = Render<CrmStatusBadge>(p => p.Add(b => b.Text, "ignored").AddChildContent("<b>rich</b>"));
        cut.Find(".crm-status b").TextContent.ShouldBe("rich");
        cut.Markup.ShouldNotContain("ignored");
    }

    // ---- CrmAvatar ----------------------------------------------------------

    [Fact]
    public void Avatar_RendersInitials_AndBackgroundStyle()
    {
        var cut = Render<CrmAvatar>(p => p.Add(a => a.Name, "Anna Smirnova").Add(a => a.Size, 40));

        var span = cut.Find("span.crm-avatar");
        span.TextContent.ShouldBe("AS");
        var style = span.GetAttribute("style")!;
        style.ShouldContain("linear-gradient");
        style.ShouldContain("width:40px");
    }

    [Fact]
    public void Avatar_EmptyName_ShowsQuestionMark()
    {
        var cut = Render<CrmAvatar>(p => p.Add(a => a.Name, ""));
        cut.Find("span.crm-avatar").TextContent.ShouldBe("?");
    }

    // ---- CrmEmptyState ------------------------------------------------------

    [Fact]
    public void EmptyState_RendersTitleDescriptionAndIcon()
    {
        var cut = Render<CrmEmptyState>(p => p
            .Add(e => e.Icon, "bi-inbox")
            .Add(e => e.Title, "Вакансий пока нет")
            .Add(e => e.Description, "Привяжите первую вакансию"));

        cut.Find(".crm-empty-title").TextContent.ShouldBe("Вакансий пока нет");
        cut.Find(".crm-empty-desc").TextContent.ShouldContain("Привяжите");
        cut.Find(".crm-empty-icon i").ClassList.ShouldContain("bi-inbox");
    }

    [Fact]
    public void EmptyState_RendersActionsSlot()
    {
        var cut = Render<CrmEmptyState>(p => p
            .Add(e => e.Title, "Пусто")
            .AddChildContent("<button class=\"cta\">Создать</button>"));

        cut.Find(".crm-empty-actions .cta").TextContent.ShouldBe("Создать");
    }
}
