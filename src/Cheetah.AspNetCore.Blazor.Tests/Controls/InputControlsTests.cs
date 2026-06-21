using Bunit;
using Cheetah.AspNetCore.Blazor.Controls;
using Microsoft.AspNetCore.Components.Web;

namespace Cheetah.AspNetCore.Blazor.Tests.Controls;

public class InputControlsTests : BunitContext
{
    // ---- CrmTextBox ---------------------------------------------------------

    [Fact]
    public void TextBox_RendersLabelValueAndPlaceholder()
    {
        var cut = Render<CrmTextBox>(p => p
            .Add(t => t.Label, "Имя")
            .Add(t => t.Value, "Иван")
            .Add(t => t.Placeholder, "введите")
            .Add(t => t.MaxLength, 100));

        cut.Find("label").TextContent.ShouldContain("Имя");
        var input = cut.Find("input");
        input.GetAttribute("value").ShouldBe("Иван");
        input.GetAttribute("placeholder").ShouldBe("введите");
        input.GetAttribute("maxlength").ShouldBe("100");
    }

    [Fact]
    public void TextBox_Required_AddsRequiredLabelClass()
    {
        var cut = Render<CrmTextBox>(p => p.Add(t => t.Label, "Имя").Add(t => t.Required, true));
        cut.Find("label").ClassList.ShouldContain("crm-label-required");
    }

    [Fact]
    public void TextBox_WithError_MarksInputInvalid_AndShowsFeedback()
    {
        var cut = Render<CrmTextBox>(p => p.Add(t => t.Error, "Обязательно"));

        cut.Find("input").ClassList.ShouldContain("is-invalid");
        cut.Find(".invalid-feedback").TextContent.ShouldBe("Обязательно");
    }

    [Fact]
    public void TextBox_Hint_ShownWhenNoError()
    {
        var cut = Render<CrmTextBox>(p => p.Add(t => t.Hint, "подсказка"));
        cut.Find(".form-text").TextContent.ShouldBe("подсказка");
    }

    [Fact]
    public void TextBox_Input_RaisesValueChanged()
    {
        string? captured = null;
        var cut = Render<CrmTextBox>(p => p.Add(t => t.ValueChanged, (string? v) => captured = v));

        cut.Find("input").Input("привет");

        captured.ShouldBe("привет");
    }

    // ---- CrmTextArea --------------------------------------------------------

    [Fact]
    public void TextArea_RendersRowsAndValue()
    {
        var cut = Render<CrmTextArea>(p => p.Add(t => t.Rows, 5).Add(t => t.Value, "текст"));

        var ta = cut.Find("textarea");
        ta.GetAttribute("rows").ShouldBe("5");
        ta.TextContent.ShouldContain("текст");
    }

    [Fact]
    public void TextArea_Input_RaisesValueChanged()
    {
        string? captured = null;
        var cut = Render<CrmTextArea>(p => p.Add(t => t.ValueChanged, (string? v) => captured = v));

        cut.Find("textarea").Input("abc");

        captured.ShouldBe("abc");
    }

    // ---- CrmNumberBox -------------------------------------------------------

    [Fact]
    public void NumberBox_RendersNumberInputWithStep()
    {
        var cut = Render<CrmNumberBox<int>>(p => p.Add(n => n.Value, 7).Add(n => n.Step, "5"));

        var input = cut.Find("input");
        input.GetAttribute("type").ShouldBe("number");
        input.GetAttribute("step").ShouldBe("5");
        input.GetAttribute("value").ShouldBe("7");
    }

    [Fact]
    public void NumberBox_Input_ParsesAndRaisesValueChanged()
    {
        int? captured = null;
        var cut = Render<CrmNumberBox<int>>(p => p.Add(n => n.ValueChanged, (int? v) => captured = v));

        cut.Find("input").Input("42");

        captured.ShouldBe(42);
    }

    [Fact]
    public void NumberBox_EmptyInput_RaisesNull()
    {
        int? captured = 99;
        var cut = Render<CrmNumberBox<int>>(p => p.Add(n => n.ValueChanged, (int? v) => captured = v));

        cut.Find("input").Input("");

        captured.ShouldBeNull();
    }

    // ---- CrmPasswordBox -----------------------------------------------------

    [Fact]
    public void PasswordBox_StartsMaskedAndTogglesToText()
    {
        var cut = Render<CrmPasswordBox>(p => p.Add(b => b.AutoComplete, "new-password"));

        cut.Find("input").GetAttribute("type").ShouldBe("password");
        cut.Find("input").GetAttribute("autocomplete").ShouldBe("new-password");

        cut.Find("button").Click();

        cut.Find("input").GetAttribute("type").ShouldBe("text");
    }

    [Fact]
    public void PasswordBox_Input_RaisesValueChanged()
    {
        string? captured = null;
        var cut = Render<CrmPasswordBox>(p => p.Add(b => b.ValueChanged, (string? v) => captured = v));

        cut.Find("input").Input("secret");

        captured.ShouldBe("secret");
    }

    // ---- CrmSearchBox -------------------------------------------------------

    [Fact]
    public void SearchBox_RendersSearchInput()
    {
        var cut = Render<CrmSearchBox>(p => p.Add(s => s.Placeholder, "найти"));
        cut.Find("input").GetAttribute("type").ShouldBe("search");
    }

    [Fact]
    public void SearchBox_EnterKey_RaisesOnSearchWithCurrentValue()
    {
        string? searched = null;
        var cut = Render<CrmSearchBox>(p => p.Add(s => s.Value, "запрос").Add(s => s.OnSearch, (string? v) => searched = v));

        cut.Find("input[type=search]").KeyDown(new KeyboardEventArgs { Key = "Enter" });

        searched.ShouldBe("запрос");
    }

    [Fact]
    public void SearchBox_ClearButton_ResetsValueAndSearch()
    {
        string? valueChanged = "x";
        string? searched = "x";
        var cut = Render<CrmSearchBox>(p => p
            .Add(s => s.Value, "abc")
            .Add(s => s.ValueChanged, (string? v) => valueChanged = v)
            .Add(s => s.OnSearch, (string? v) => searched = v));

        // кнопка очистки видна только при непустом значении
        cut.Find("button").Click();

        valueChanged.ShouldBeNull();
        searched.ShouldBeNull();
    }
}
