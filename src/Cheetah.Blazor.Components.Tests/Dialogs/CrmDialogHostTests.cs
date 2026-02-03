using Bunit;
using Cheetah.Blazor.Components.Dialogs;
using Cheetah.Blazor.Components.Tests.Dialogs.TestComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Blazor.Components.Tests.Dialogs;

public class CrmDialogHostTests : TestContext
{
    [Fact]
    public void ShouldRenderModalWithTitle()
    {
        // Arrange
        var options = new DialogOptions { Title = "Test Dialog" };
        var instance = new DialogInstance(typeof(SimpleDialogContent), null, options);

        // Act
        var cut = RenderComponent<CrmDialogHost>(parameters => parameters
            .Add(p => p.Instance, instance));

        // Assert
        cut.Find(".modal-title").TextContent.ShouldBe("Test Dialog");
    }

    [Fact]
    public void ShouldRenderDialogContent()
    {
        // Arrange
        var instance = new DialogInstance(typeof(SimpleDialogContent), null, null);

        // Act
        var cut = RenderComponent<CrmDialogHost>(parameters => parameters
            .Add(p => p.Instance, instance));

        // Assert
        cut.Find(".test-content").ShouldNotBeNull();
        cut.Find(".test-content p").TextContent.ShouldBe("Dialog content");
    }

    [Fact]
    public void ShouldPassParametersToContent()
    {
        // Arrange
        var parameters = new Dictionary<string, object?> { ["Message"] = "Hello World" };
        var instance = new DialogInstance(typeof(SimpleDialogContent), parameters, null);

        // Act
        var cut = RenderComponent<CrmDialogHost>(parameters => parameters
            .Add(p => p.Instance, instance));

        // Assert
        cut.Find(".message").TextContent.ShouldBe("Hello World");
    }

    [Fact]
    public void ShouldRenderDefaultButtons()
    {
        // Arrange
        var instance = new DialogInstance(typeof(SimpleDialogContent), null, null);

        // Act
        var cut = RenderComponent<CrmDialogHost>(parameters => parameters
            .Add(p => p.Instance, instance));

        // Assert
        var buttons = cut.FindAll(".modal-footer button");
        buttons.Count.ShouldBe(2); // Cancel + Save from SaveCancel preset
    }

    [Fact]
    public void ShouldRenderCustomButtons()
    {
        // Arrange
        var options = new DialogOptions { Buttons = DialogButtons.YesNo };
        var instance = new DialogInstance(typeof(SimpleDialogContent), null, options);

        // Act
        var cut = RenderComponent<CrmDialogHost>(parameters => parameters
            .Add(p => p.Instance, instance));

        // Assert
        var buttons = cut.FindAll(".modal-footer button");
        buttons.Count.ShouldBe(2);
        buttons[0].TextContent.ShouldContain("Нет");
        buttons[1].TextContent.ShouldContain("Да");
    }

    [Fact]
    public void CancelButton_ShouldInvokeOnClose()
    {
        // Arrange
        var instance = new DialogInstance(typeof(SimpleDialogContent), null, null);
        DialogResult? closedResult = null;

        var cut = RenderComponent<CrmDialogHost>(parameters => parameters
            .Add(p => p.Instance, instance)
            .Add(p => p.OnClose, EventCallback.Factory.Create<DialogResult>(this, r => closedResult = r)));

        // Act - click cancel button (first button in SaveCancel)
        var cancelButton = cut.FindAll(".modal-footer button")[0];
        cancelButton.Click();

        // Assert
        closedResult.ShouldNotBeNull();
        closedResult!.Confirmed.ShouldBeFalse();
    }

    [Fact]
    public void CloseButtonInHeader_ShouldInvokeOnClose()
    {
        // Arrange
        var options = new DialogOptions { Title = "Test", ShowCloseButton = true };
        var instance = new DialogInstance(typeof(SimpleDialogContent), null, options);
        DialogResult? closedResult = null;

        var cut = RenderComponent<CrmDialogHost>(parameters => parameters
            .Add(p => p.Instance, instance)
            .Add(p => p.OnClose, EventCallback.Factory.Create<DialogResult>(this, r => closedResult = r)));

        // Act - click close button in header
        var closeButton = cut.Find(".btn-close");
        closeButton.Click();

        // Assert
        closedResult.ShouldNotBeNull();
        closedResult!.Confirmed.ShouldBeFalse();
    }

    [Fact]
    public void ShouldRespectModalSize()
    {
        // Arrange
        var options = new DialogOptions { Size = Modals.CrmModal.ModalSize.Large };
        var instance = new DialogInstance(typeof(SimpleDialogContent), null, options);

        // Act
        var cut = RenderComponent<CrmDialogHost>(parameters => parameters
            .Add(p => p.Instance, instance));

        // Assert
        cut.Find(".modal-dialog").ClassList.ShouldContain("modal-lg");
    }

    [Fact]
    public void ShouldRespectCenteredOption()
    {
        // Arrange
        var options = new DialogOptions { Centered = true };
        var instance = new DialogInstance(typeof(SimpleDialogContent), null, options);

        // Act
        var cut = RenderComponent<CrmDialogHost>(parameters => parameters
            .Add(p => p.Instance, instance));

        // Assert
        cut.Find(".modal-dialog").ClassList.ShouldContain("modal-dialog-centered");
    }

    [Fact]
    public void ShouldRespectScrollableOption()
    {
        // Arrange
        var options = new DialogOptions { Scrollable = true };
        var instance = new DialogInstance(typeof(SimpleDialogContent), null, options);

        // Act
        var cut = RenderComponent<CrmDialogHost>(parameters => parameters
            .Add(p => p.Instance, instance));

        // Assert
        cut.Find(".modal-dialog").ClassList.ShouldContain("modal-dialog-scrollable");
    }

    [Fact]
    public void ConfirmButton_WithSimpleComponent_ShouldCloseWithOk()
    {
        // Arrange
        var instance = new DialogInstance(typeof(SimpleDialogContent), null, null);
        DialogResult? closedResult = null;

        var cut = RenderComponent<CrmDialogHost>(parameters => parameters
            .Add(p => p.Instance, instance)
            .Add(p => p.OnClose, EventCallback.Factory.Create<DialogResult>(this, r => closedResult = r)));

        // Act - click confirm button (second button in SaveCancel)
        var confirmButton = cut.FindAll(".modal-footer button")[1];
        confirmButton.Click();

        // Assert
        closedResult.ShouldNotBeNull();
        closedResult!.Confirmed.ShouldBeTrue();
    }

    [Fact]
    public void ConfirmButton_WithFormComponent_ShouldCallSubmitAsync()
    {
        // Arrange
        var parameters = new Dictionary<string, object?> { ["InitialName"] = "Test Name" };
        var instance = new DialogInstance(typeof(FormDialogContent), parameters, null);
        DialogResult? closedResult = null;

        var cut = RenderComponent<CrmDialogHost>(parameters => parameters
            .Add(p => p.Instance, instance)
            .Add(p => p.OnClose, EventCallback.Factory.Create<DialogResult>(this, r => closedResult = r)));

        // Act - click confirm button
        var confirmButton = cut.FindAll(".modal-footer button")[1];
        confirmButton.Click();

        // Assert
        closedResult.ShouldNotBeNull();
        closedResult!.Confirmed.ShouldBeTrue();
        var data = closedResult.GetData<FormDialogResult>();
        data.ShouldNotBeNull();
        data!.Name.ShouldBe("Test Name");
    }

    [Fact]
    public void ConfirmButton_WhenFormFails_ShouldNotClose()
    {
        // Arrange
        var parameters = new Dictionary<string, object?>
        {
            ["InitialName"] = "Test",
            ["ShouldFail"] = true
        };
        var instance = new DialogInstance(typeof(FormDialogContent), parameters, null);
        DialogResult? closedResult = null;

        var cut = RenderComponent<CrmDialogHost>(parameters => parameters
            .Add(p => p.Instance, instance)
            .Add(p => p.OnClose, EventCallback.Factory.Create<DialogResult>(this, r => closedResult = r)));

        // Act - click confirm button
        var confirmButton = cut.FindAll(".modal-footer button")[1];
        confirmButton.Click();

        // Assert
        closedResult.ShouldBeNull(); // Dialog should not close when form fails
    }

    [Fact]
    public void Dispose_ShouldNotThrow()
    {
        // Arrange
        var instance = new DialogInstance(typeof(SimpleDialogContent), null, null);
        var cut = RenderComponent<CrmDialogHost>(parameters => parameters
            .Add(p => p.Instance, instance));

        // Act
        var act = () => cut.Dispose();

        // Assert
        Should.NotThrow(act);
    }
}
