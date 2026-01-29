using Cheetah.Blazor.Components.Dialogs;
using Microsoft.AspNetCore.Components.Forms;

namespace Cheetah.Blazor.Components.Tests.Dialogs;

public class DialogInstanceTests
{
    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange
        var componentType = typeof(TestComponent);
        var parameters = new Dictionary<string, object?> { ["Id"] = 1 };
        var options = new DialogOptions { Title = "Test" };

        // Act
        var instance = new DialogInstance(componentType, parameters, options);

        // Assert
        instance.Id.Should().NotBeEmpty();
        instance.ComponentType.Should().Be(componentType);
        instance.Parameters.Should().ContainKey("Id");
        instance.Parameters["Id"].Should().Be(1);
        instance.Options.Title.Should().Be("Test");
    }

    [Fact]
    public void Constructor_WithNullParameters_ShouldUseEmptyDictionary()
    {
        // Arrange
        var componentType = typeof(TestComponent);

        // Act
        var instance = new DialogInstance(componentType, null, null);

        // Assert
        instance.Parameters.Should().NotBeNull();
        instance.Parameters.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithNullOptions_ShouldUseDefaultOptions()
    {
        // Arrange
        var componentType = typeof(TestComponent);

        // Act
        var instance = new DialogInstance(componentType, null, null);

        // Assert
        instance.Options.Should().NotBeNull();
        instance.Options.Title.Should().BeNull();
    }

    [Fact]
    public void Id_ShouldBeUnique()
    {
        // Arrange & Act
        var instance1 = new DialogInstance(typeof(TestComponent), null, null);
        var instance2 = new DialogInstance(typeof(TestComponent), null, null);

        // Assert
        instance1.Id.Should().NotBe(instance2.Id);
    }

    [Fact]
    public void IsFormValid_WhenNoEditContext_ShouldReturnTrue()
    {
        // Arrange
        var instance = new DialogInstance(typeof(TestComponent), null, null);

        // Act
        var isValid = instance.IsFormValid;

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void IsFormValid_WithValidEditContext_ShouldReturnTrue()
    {
        // Arrange
        var model = new TestModel { Name = "Test" };
        var editContext = new EditContext(model);
        var instance = new DialogInstance(typeof(TestComponent), null, null);

        // Act
        instance.RegisterEditContext(editContext);
        var isValid = instance.IsFormValid;

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void RegisterEditContext_ShouldRaiseOnStateChanged()
    {
        // Arrange
        var instance = new DialogInstance(typeof(TestComponent), null, null);
        var eventRaised = false;
        instance.OnStateChanged += () => eventRaised = true;
        var editContext = new EditContext(new TestModel());

        // Act
        instance.RegisterEditContext(editContext);

        // Assert
        eventRaised.Should().BeTrue();
    }

    [Fact]
    public void RegisterEditContext_WithNull_ShouldUnsubscribeFromPrevious()
    {
        // Arrange
        var model = new TestModel();
        var editContext = new EditContext(model);
        var instance = new DialogInstance(typeof(TestComponent), null, null);
        instance.RegisterEditContext(editContext);

        var changeCount = 0;
        instance.OnStateChanged += () => changeCount++;

        // Act
        instance.RegisterEditContext(null);

        // Assert
        changeCount.Should().Be(1); // Only from RegisterEditContext(null) call
    }

    [Fact]
    public async Task Close_ShouldCompleteResultTask()
    {
        // Arrange
        var instance = new DialogInstance(typeof(TestComponent), null, null);
        var expectedResult = DialogResult.Ok("test");

        // Act
        instance.Close(expectedResult);
        var result = await instance.Result;

        // Assert
        result.Should().BeSameAs(expectedResult);
    }

    [Fact]
    public async Task Close_CalledMultipleTimes_ShouldReturnFirstResult()
    {
        // Arrange
        var instance = new DialogInstance(typeof(TestComponent), null, null);
        var firstResult = DialogResult.Ok("first");
        var secondResult = DialogResult.Ok("second");

        // Act
        instance.Close(firstResult);
        instance.Close(secondResult);
        var result = await instance.Result;

        // Assert
        result.Should().BeSameAs(firstResult);
    }

    [Fact]
    public void Dispose_ShouldNotThrow()
    {
        // Arrange
        var model = new TestModel();
        var editContext = new EditContext(model);
        var instance = new DialogInstance(typeof(TestComponent), null, null);
        instance.RegisterEditContext(editContext);

        // Act
        var act = () => instance.Dispose();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_ShouldNotThrow()
    {
        // Arrange
        var instance = new DialogInstance(typeof(TestComponent), null, null);

        // Act
        var act = () =>
        {
            instance.Dispose();
            instance.Dispose();
        };

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void OnStateChanged_WhenValidationStateChanges_ShouldBeRaised()
    {
        // Arrange
        var model = new TestModel();
        var editContext = new EditContext(model);
        var instance = new DialogInstance(typeof(TestComponent), null, null);
        instance.RegisterEditContext(editContext);

        var stateChangedCount = 0;
        instance.OnStateChanged += () => stateChangedCount++;

        // Act - trigger validation state change
        editContext.NotifyValidationStateChanged();

        // Assert
        stateChangedCount.Should().Be(1);
    }

    private class TestComponent : Microsoft.AspNetCore.Components.ComponentBase
    {
    }

    private class TestModel
    {
        public string Name { get; set; } = "";
    }
}
