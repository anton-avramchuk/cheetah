using Cheetah.Blazor.Components;
using Cheetah.Blazor.Components.Dialogs;
using Cheetah.Blazor.Components.Modals;

namespace Cheetah.Blazor.Components.Tests.Dialogs;

public class DialogFormResultTests
{
    [Fact]
    public void Ok_ShouldCreateSuccessResult()
    {
        // Arrange
        var data = "test data";

        // Act
        var result = DialogFormResult<string>.Ok(data);

        // Assert
        result.Success.ShouldBeTrue();
        result.Data.ShouldBe(data);
        result.ErrorMessage.ShouldBeNull();
    }

    [Fact]
    public void Fail_ShouldCreateFailureResult()
    {
        // Arrange
        var error = "Validation error";

        // Act
        var result = DialogFormResult<string>.Fail(error);

        // Assert
        result.Success.ShouldBeFalse();
        result.Data.ShouldBeNull();
        result.ErrorMessage.ShouldBe(error);
    }

    [Fact]
    public void Ok_WithComplexType_ShouldPreserveData()
    {
        // Arrange
        var data = new TestData { Id = 1, Name = "Test" };

        // Act
        var result = DialogFormResult<TestData>.Ok(data);

        // Assert
        result.Success.ShouldBeTrue();
        result.Data.ShouldNotBeNull();
        result.Data!.Id.ShouldBe(1);
        result.Data.Name.ShouldBe("Test");
    }

    private class TestData
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }
}

public class DialogResultTests
{
    [Fact]
    public void Ok_WithoutData_ShouldCreateConfirmedResult()
    {
        // Act
        var result = DialogResult.Ok();

        // Assert
        result.Confirmed.ShouldBeTrue();
        result.Data.ShouldBeNull();
    }

    [Fact]
    public void Ok_WithData_ShouldCreateConfirmedResultWithData()
    {
        // Arrange
        var data = new { Id = 1 };

        // Act
        var result = DialogResult.Ok(data);

        // Assert
        result.Confirmed.ShouldBeTrue();
        result.Data.ShouldBe(data);
    }

    [Fact]
    public void Cancel_ShouldCreateUnconfirmedResult()
    {
        // Act
        var result = DialogResult.Cancel();

        // Assert
        result.Confirmed.ShouldBeFalse();
        result.Data.ShouldBeNull();
    }

    [Fact]
    public void GetData_WithCorrectType_ShouldReturnData()
    {
        // Arrange
        var data = "test string";
        var result = DialogResult.Ok(data);

        // Act
        var retrieved = result.GetData<string>();

        // Assert
        retrieved.ShouldBe(data);
    }

    [Fact]
    public void GetData_WithWrongType_ShouldReturnDefault()
    {
        // Arrange
        var data = "test string";
        var result = DialogResult.Ok(data);

        // Act
        var retrieved = result.GetData<int>();

        // Assert
        retrieved.ShouldBe(default);
    }

    [Fact]
    public void GetData_WhenDataIsNull_ShouldReturnDefault()
    {
        // Arrange
        var result = DialogResult.Ok();

        // Act
        var retrieved = result.GetData<string>();

        // Assert
        retrieved.ShouldBeNull();
    }
}

public class DialogButtonTests
{
    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        // Act
        var button = new DialogButton();

        // Assert
        button.Text.ShouldBeEmpty();
        button.Icon.ShouldBeNull();
        button.Variant.ShouldBe(ColorVariant.Secondary);
        button.IsConfirm.ShouldBeFalse();
        button.IsCancel.ShouldBeFalse();
        button.DisableWhenInvalid.ShouldBeTrue();
    }

    [Fact]
    public void Properties_ShouldBeSettable()
    {
        // Arrange & Act
        var button = new DialogButton
        {
            Text = "Submit",
            Icon = "check",
            Variant = ColorVariant.Primary,
            IsConfirm = true,
            IsCancel = false,
            DisableWhenInvalid = false
        };

        // Assert
        button.Text.ShouldBe("Submit");
        button.Icon.ShouldBe("check");
        button.Variant.ShouldBe(ColorVariant.Primary);
        button.IsConfirm.ShouldBeTrue();
        button.IsCancel.ShouldBeFalse();
        button.DisableWhenInvalid.ShouldBeFalse();
    }
}

public class DialogButtonsPresetsTests
{
    [Fact]
    public void SaveCancel_ShouldHaveTwoButtons()
    {
        // Act
        var buttons = DialogButtons.SaveCancel;

        // Assert
        buttons.Count.ShouldBe(2);
    }

    [Fact]
    public void SaveCancel_ShouldHaveCancelButton()
    {
        // Act
        var buttons = DialogButtons.SaveCancel;
        var cancelButton = buttons.FirstOrDefault(b => b.IsCancel);

        // Assert
        cancelButton.ShouldNotBeNull();
        cancelButton!.Text.ShouldBe("Отмена");
        cancelButton.Variant.ShouldBe(ColorVariant.Secondary);
        cancelButton.DisableWhenInvalid.ShouldBeFalse();
    }

    [Fact]
    public void SaveCancel_ShouldHaveSaveButton()
    {
        // Act
        var buttons = DialogButtons.SaveCancel;
        var saveButton = buttons.FirstOrDefault(b => b.IsConfirm);

        // Assert
        saveButton.ShouldNotBeNull();
        saveButton!.Text.ShouldBe("Сохранить");
        saveButton.Icon.ShouldBe("check");
        saveButton.Variant.ShouldBe(ColorVariant.Primary);
        saveButton.DisableWhenInvalid.ShouldBeTrue();
    }

    [Fact]
    public void OkCancel_ShouldHaveTwoButtons()
    {
        // Act
        var buttons = DialogButtons.OkCancel;

        // Assert
        buttons.Count.ShouldBe(2);
        buttons.ShouldContain(b => b.IsCancel && b.Text == "Отмена");
        buttons.ShouldContain(b => b.IsConfirm && b.Text == "OK");
    }

    [Fact]
    public void YesNo_ShouldHaveTwoButtons()
    {
        // Act
        var buttons = DialogButtons.YesNo;

        // Assert
        buttons.Count.ShouldBe(2);
        buttons.ShouldContain(b => b.IsCancel && b.Text == "Нет");
        buttons.ShouldContain(b => b.IsConfirm && b.Text == "Да");
    }

    [Fact]
    public void YesNo_BothButtonsShouldNotDisableWhenInvalid()
    {
        // Act
        var buttons = DialogButtons.YesNo;

        // Assert
        foreach (var b in buttons)
            b.DisableWhenInvalid.ShouldBeFalse();
    }

    [Fact]
    public void DeleteCancel_ShouldHaveDeleteButtonWithDangerVariant()
    {
        // Act
        var buttons = DialogButtons.DeleteCancel;
        var deleteButton = buttons.FirstOrDefault(b => b.IsConfirm);

        // Assert
        deleteButton.ShouldNotBeNull();
        deleteButton!.Text.ShouldBe("Удалить");
        deleteButton.Icon.ShouldBe("trash");
        deleteButton.Variant.ShouldBe(ColorVariant.Danger);
    }
}

public class DialogOptionsTests
{
    [Fact]
    public void DefaultValues_ShouldBeCorrect()
    {
        // Act
        var options = new DialogOptions();

        // Assert
        options.Title.ShouldBeNull();
        options.Size.ShouldBe(CrmModal.ModalSize.Default);
        options.Centered.ShouldBeFalse();
        options.Scrollable.ShouldBeFalse();
        options.ShowCloseButton.ShouldBeTrue();
        options.CloseOnBackdropClick.ShouldBeFalse();
        options.Buttons.ShouldBeNull();
    }

    [Fact]
    public void Properties_ShouldBeSettable()
    {
        // Arrange & Act
        var options = new DialogOptions
        {
            Title = "Test Dialog",
            Size = CrmModal.ModalSize.Large,
            Centered = true,
            Scrollable = true,
            ShowCloseButton = false,
            CloseOnBackdropClick = true,
            Buttons = DialogButtons.YesNo
        };

        // Assert
        options.Title.ShouldBe("Test Dialog");
        options.Size.ShouldBe(CrmModal.ModalSize.Large);
        options.Centered.ShouldBeTrue();
        options.Scrollable.ShouldBeTrue();
        options.ShowCloseButton.ShouldBeFalse();
        options.CloseOnBackdropClick.ShouldBeTrue();
        options.Buttons.Count.ShouldBe(DialogButtons.YesNo.Count);
    }
}
