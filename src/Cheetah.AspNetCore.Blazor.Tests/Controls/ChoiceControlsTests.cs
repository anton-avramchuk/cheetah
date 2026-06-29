using Bunit;
using Cheetah.AspNetCore.Blazor.Controls;
using Microsoft.AspNetCore.Components.Forms;

namespace Cheetah.AspNetCore.Blazor.Tests.Controls;

public class ChoiceControlsTests : BunitContext
{
    // ---- CrmCheckBox --------------------------------------------------------

    [Fact]
    public void CheckBox_ReflectsCheckedState_AndLabel()
    {
        var cut = Render<CrmCheckBox>(p => p.Add(c => c.Value, true).Add(c => c.Label, "Согласен"));

        var input = cut.Find("input[type=checkbox]");
        input.ClassList.ShouldContain("form-check-input");
        input.HasAttribute("checked").ShouldBeTrue();
        cut.Find("label.form-check-label").TextContent.ShouldContain("Согласен");
    }

    [Fact]
    public void CheckBox_Change_RaisesValueChanged()
    {
        bool? captured = null;
        var cut = Render<CrmCheckBox>(p => p.Add(c => c.Value, true).Add(c => c.ValueChanged, (bool v) => captured = v));

        cut.Find("input").Change(false);

        captured.ShouldBe(false);
    }

    [Fact]
    public void CheckBox_WithError_ShowsInvalid()
    {
        var cut = Render<CrmCheckBox>(p => p.Add(c => c.Error, "нужно"));
        cut.Find("input").ClassList.ShouldContain("is-invalid");
        cut.Find(".invalid-feedback").TextContent.ShouldBe("нужно");
    }

    // ---- CrmSwitch ----------------------------------------------------------

    [Fact]
    public void Switch_RendersAsSwitchRole()
    {
        var cut = Render<CrmSwitch>(p => p.Add(s => s.Label, "Активен").Add(s => s.Value, true));

        cut.Find(".form-switch").ShouldNotBeNull();
        var input = cut.Find("input");
        input.GetAttribute("role").ShouldBe("switch");
        input.HasAttribute("checked").ShouldBeTrue();
    }

    [Fact]
    public void Switch_Change_RaisesValueChanged()
    {
        bool? captured = null;
        var cut = Render<CrmSwitch>(p => p.Add(s => s.ValueChanged, (bool v) => captured = v));

        cut.Find("input").Change(true);

        captured.ShouldBe(true);
    }

    // ---- CrmSelect ----------------------------------------------------------

    [Fact]
    public void Select_RendersPlaceholder_AndOptionsWhenOpen()
    {
        var cut = Render<CrmSelect<string>>(p => p
            .Add(s => s.Items, new[] { "a", "b", "c" })
            .Add(s => s.Placeholder, "— выбор —"));

        // placeholder в свёрнутом виде, меню закрыто → опций нет
        cut.Find(".crm-select__placeholder").TextContent.ShouldContain("— выбор —");
        cut.FindAll(".crm-select__option").Count.ShouldBe(0);

        // открыли меню → 3 опции
        cut.Find(".crm-select__toggle").Click();
        cut.FindAll(".crm-select__option").Count.ShouldBe(3);
    }

    [Fact]
    public void Select_Click_RaisesValueChangedWithMatchedItem()
    {
        string? captured = null;
        var cut = Render<CrmSelect<string>>(p => p
            .Add(s => s.Items, new[] { "a", "b", "c" })
            .Add(s => s.Value, "a")
            .Add(s => s.ValueChanged, (string? v) => captured = v));

        cut.Find(".crm-select__toggle").Click();
        cut.FindAll(".crm-select__option")[1].Click();

        captured.ShouldBe("b");
    }

    [Fact]
    public void Select_WithError_ShowsInvalid()
    {
        var cut = Render<CrmSelect<string>>(p => p
            .Add(s => s.Items, new[] { "a" })
            .Add(s => s.Error, "выберите"));

        cut.Find(".crm-select__toggle").ClassList.ShouldContain("is-invalid");
        cut.Find(".invalid-feedback").TextContent.ShouldBe("выберите");
    }

    [Fact]
    public void Select_ItemTemplate_RendersCustomMarkup()
    {
        var cut = Render<CrmSelect<string>>(p => p
            .Add(s => s.Items, new[] { "a" })
            .Add(s => s.ItemTemplate, item => builder => builder.AddMarkupContent(0, $"<span class=\"flag\">{item}</span>")));

        cut.Find(".crm-select__toggle").Click();
        cut.Find(".crm-select__option .flag").TextContent.ShouldBe("a");
    }

    [Fact]
    public void Select_Multiple_TogglesValues_AndKeepsMenuOpen()
    {
        IReadOnlyCollection<string>? captured = null;
        var cut = Render<CrmSelect<string>>(p => p
            .Add(s => s.Multiple, true)
            .Add(s => s.Items, new[] { "a", "b", "c" })
            .Add(s => s.Values, new[] { "a" })
            .Add(s => s.ValuesChanged, (IReadOnlyCollection<string> v) => captured = v));

        cut.Find(".crm-select__toggle").Click();
        cut.FindAll(".crm-select__option")[1].Click(); // выбрать "b"

        captured.ShouldBe(new[] { "a", "b" });
        // меню остаётся открытым в множественном режиме
        cut.FindAll(".crm-select__option").Count.ShouldBe(3);
    }

    // ---- CrmRadioGroup ------------------------------------------------------

    [Fact]
    public void RadioGroup_RendersOneInputPerItem_Inline()
    {
        var cut = Render<CrmRadioGroup<string>>(p => p
            .Add(r => r.Items, new[] { "x", "y" })
            .Add(r => r.Inline, true));

        cut.FindAll("input[type=radio]").Count.ShouldBe(2);
        cut.Find(".form-check").ClassList.ShouldContain("form-check-inline");
    }

    [Fact]
    public void RadioGroup_Select_RaisesValueChanged()
    {
        string? captured = null;
        var cut = Render<CrmRadioGroup<string>>(p => p
            .Add(r => r.Items, new[] { "x", "y" })
            .Add(r => r.ValueChanged, (string? v) => captured = v));

        cut.FindAll("input[type=radio]")[1].Change(true);

        captured.ShouldBe("y");
    }

    // ---- CrmDatePicker ------------------------------------------------------

    [Fact]
    public void DatePicker_FormatsValueAsIsoDate()
    {
        var cut = Render<CrmDatePicker>(p => p.Add(d => d.Value, new DateOnly(2024, 5, 1)));

        var input = cut.Find("input");
        input.GetAttribute("type").ShouldBe("date");
        input.GetAttribute("value").ShouldBe("2024-05-01");
    }

    [Fact]
    public void DatePicker_Change_RaisesValueChanged()
    {
        DateOnly? captured = null;
        var cut = Render<CrmDatePicker>(p => p.Add(d => d.ValueChanged, (DateOnly? v) => captured = v));

        cut.Find("input").Change("2024-05-01");

        captured.ShouldBe(new DateOnly(2024, 5, 1));
    }

    // ---- CrmDateTimePicker --------------------------------------------------

    [Fact]
    public void DateTimePicker_FormatsValueAsLocalDateTime()
    {
        var cut = Render<CrmDateTimePicker>(p => p.Add(d => d.Value, new DateTime(2024, 5, 1, 10, 30, 0)));

        var input = cut.Find("input");
        input.GetAttribute("type").ShouldBe("datetime-local");
        input.GetAttribute("value").ShouldBe("2024-05-01T10:30");
    }

    [Fact]
    public void DateTimePicker_Change_RaisesValueChanged()
    {
        DateTime? captured = null;
        var cut = Render<CrmDateTimePicker>(p => p.Add(d => d.ValueChanged, (DateTime? v) => captured = v));

        cut.Find("input").Change("2024-05-01T10:30");

        captured.ShouldBe(new DateTime(2024, 5, 1, 10, 30, 0));
    }

    // ---- CrmFileUpload ------------------------------------------------------

    [Fact]
    public void FileUpload_RendersFileInputWithAccept()
    {
        var cut = Render<CrmFileUpload>(p => p.Add(f => f.Accept, ".pdf").Add(f => f.Label, "Резюме"));

        var input = cut.Find("input[type=file]");
        input.ClassList.ShouldContain("form-control");
        input.GetAttribute("accept").ShouldBe(".pdf");
    }

    [Fact]
    public void FileUpload_SingleFile_RaisesOnFileSelected()
    {
        IBrowserFile? captured = null;
        var cut = Render<CrmFileUpload>(p => p.Add(f => f.OnFileSelected, (IBrowserFile f) => captured = f));

        cut.FindComponent<InputFile>().UploadFiles(InputFileContent.CreateFromText("hello", "resume.txt"));

        captured.ShouldNotBeNull();
        captured!.Name.ShouldBe("resume.txt");
    }

    [Fact]
    public void FileUpload_WithError_ShowsInvalid()
    {
        var cut = Render<CrmFileUpload>(p => p.Add(f => f.Error, "нужен файл"));
        cut.Find("input[type=file]").ClassList.ShouldContain("is-invalid");
    }
}
