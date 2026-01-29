using Cheetah.Blazor.Components.Buttons;
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
        result.Success.Should().BeTrue();
        result.Data.Should().Be(data);
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Fail_ShouldCreateFailureResult()
    {
        // Arrange
        var error = "Validation error";

        // Act
        var result = DialogFormResult<string>.Fail(error);

        // Assert
        result.Success.Should().BeFalse();
        result.Data.Should().BeNull();
        result.ErrorMessage.Should().Be(error);
    }

    [Fact]
    public void Ok_WithComplexType_ShouldPreserveData()
    {
        // Arrange
        var data = new TestData { Id = 1, Name = "Test" };

        // Act
        var result = DialogFormResult<TestData>.Ok(data);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(1);
        result.Data.Name.Should().Be("Test");
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
        result.Confirmed.Should().BeTrue();
        result.Data.Should().BeNull();
    }

    [Fact]
    public void Ok_WithData_ShouldCreateConfirmedResultWithData()
    {
        // Arrange
        var data = new { Id = 1 };

        // Act
        var result = DialogResult.Ok(data);

        // Assert
        result.Confirmed.Should().BeTrue();
        result.Data.Should().Be(data);
    }

    [Fact]
    public void Cancel_ShouldCreateUnconfirmedResult()
    {
        // Act
        var result = DialogResult.Cancel();

        // Assert
        result.Confirmed.Should().BeFalse();
        result.Data.Should().BeNull();
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
        retrieved.Should().Be(data);
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
        retrieved.Should().Be(default);
    }

    [Fact]
    public void GetData_WhenDataIsNull_ShouldReturnDefault()
    {
        // Arrange
        var result = DialogResult.Ok();

        // Act
        var retrieved = result.GetData<string>();

        // Assert
        retrieved.Should().BeNull();
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
        button.Text.Should().BeEmpty();
        button.Icon.Should().BeNull();
        button.Variant.Should().Be(CrmButton.ButtonVariant.Secondary);
        button.IsConfirm.Should().BeFalse();
        button.IsCancel.Should().BeFalse();
        button.DisableWhenInvalid.Should().BeTrue();
    }

    [Fact]
    public void Properties_ShouldBeSettable()
    {
        // Arrange & Act
        var button = new DialogButton
        {
            Text = "Submit",
            Icon = "check",
            Variant = CrmButton.ButtonVariant.Primary,
            IsConfirm = true,
            IsCancel = false,
            DisableWhenInvalid = false
        };

        // Assert
        button.Text.Should().Be("Submit");
        button.Icon.Should().Be("check");
        button.Variant.Should().Be(CrmButton.ButtonVariant.Primary);
        button.IsConfirm.Should().BeTrue();
        button.IsCancel.Should().BeFalse();
        button.DisableWhenInvalid.Should().BeFalse();
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
        buttons.Should().HaveCount(2);
    }

    [Fact]
    public void SaveCancel_ShouldHaveCancelButton()
    {
        // Act
        var buttons = DialogButtons.SaveCancel;
        var cancelButton = buttons.FirstOrDefault(b => b.IsCancel);

        // Assert
        cancelButton.Should().NotBeNull();
        cancelButton!.Text.Should().Be("Отмена");
        cancelButton.Variant.Should().Be(CrmButton.ButtonVariant.Secondary);
        cancelButton.DisableWhenInvalid.Should().BeFalse();
    }

    [Fact]
    public void SaveCancel_ShouldHaveSaveButton()
    {
        // Act
        var buttons = DialogButtons.SaveCancel;
        var saveButton = buttons.FirstOrDefault(b => b.IsConfirm);

        // Assert
        saveButton.Should().NotBeNull();
        saveButton!.Text.Should().Be("Сохранить");
        saveButton.Icon.Should().Be("check");
        saveButton.Variant.Should().Be(CrmButton.ButtonVariant.Primary);
        saveButton.DisableWhenInvalid.Should().BeTrue();
    }

    [Fact]
    public void OkCancel_ShouldHaveTwoButtons()
    {
        // Act
        var buttons = DialogButtons.OkCancel;

        // Assert
        buttons.Should().HaveCount(2);
        buttons.Should().Contain(b => b.IsCancel && b.Text == "Отмена");
        buttons.Should().Contain(b => b.IsConfirm && b.Text == "OK");
    }

    [Fact]
    public void YesNo_ShouldHaveTwoButtons()
    {
        // Act
        var buttons = DialogButtons.YesNo;

        // Assert
        buttons.Should().HaveCount(2);
        buttons.Should().Contain(b => b.IsCancel && b.Text == "Нет");
        buttons.Should().Contain(b => b.IsConfirm && b.Text == "Да");
    }

    [Fact]
    public void YesNo_BothButtonsShouldNotDisableWhenInvalid()
    {
        // Act
        var buttons = DialogButtons.YesNo;

        // Assert
        buttons.Should().AllSatisfy(b => b.DisableWhenInvalid.Should().BeFalse());
    }

    [Fact]
    public void DeleteCancel_ShouldHaveDeleteButtonWithDangerVariant()
    {
        // Act
        var buttons = DialogButtons.DeleteCancel;
        var deleteButton = buttons.FirstOrDefault(b => b.IsConfirm);

        // Assert
        deleteButton.Should().NotBeNull();
        deleteButton!.Text.Should().Be("Удалить");
        deleteButton.Icon.Should().Be("trash");
        deleteButton.Variant.Should().Be(CrmButton.ButtonVariant.Danger);
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
        options.Title.Should().BeNull();
        options.Size.Should().Be(CrmModal.ModalSize.Default);
        options.Centered.Should().BeFalse();
        options.Scrollable.Should().BeFalse();
        options.ShowCloseButton.Should().BeTrue();
        options.CloseOnBackdropClick.Should().BeFalse();
        options.Buttons.Should().BeNull();
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
        options.Title.Should().Be("Test Dialog");
        options.Size.Should().Be(CrmModal.ModalSize.Large);
        options.Centered.Should().BeTrue();
        options.Scrollable.Should().BeTrue();
        options.ShowCloseButton.Should().BeFalse();
        options.CloseOnBackdropClick.Should().BeTrue();
        options.Buttons.Should().BeEquivalentTo(DialogButtons.YesNo);
    }
}
