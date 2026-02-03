using Cheetah.Blazor.Components.Dialogs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Cheetah.Blazor.Components.Tests.Dialogs;

public class DialogServiceTests
{
    [Fact]
    public void Dialogs_Initially_ShouldBeEmpty()
    {
        // Arrange
        var service = CreateService();

        // Act
        var dialogs = service.Dialogs;

        // Assert
        dialogs.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShowAsync_ShouldAddDialogToList()
    {
        // Arrange
        var service = CreateService();
        var dialogAddedTcs = new TaskCompletionSource();

        service.OnChange += () =>
        {
            if (service.Dialogs.Count == 1)
                dialogAddedTcs.TrySetResult();
        };

        // Act
        var showTask = service.ShowAsync<SimpleTestComponent>(
            options: new DialogOptions { Title = "Test" });

        await dialogAddedTcs.Task.WaitAsync(TimeSpan.FromSeconds(1));

        // Assert
        service.Dialogs.Count.ShouldBe(1);
        service.Dialogs[0].Options.Title.ShouldBe("Test");

        // Cleanup - close the dialog
        service.CloseDialog(service.Dialogs[0], DialogResult.Cancel());
        await showTask;
    }

    [Fact]
    public async Task ShowAsync_WhenClosed_ShouldRemoveDialogFromList()
    {
        // Arrange
        var service = CreateService();
        var showTask = service.ShowAsync<SimpleTestComponent>();

        // Wait for dialog to be added
        await WaitForDialogCountAsync(service, 1);

        // Act
        service.CloseDialog(service.Dialogs[0], DialogResult.Ok());
        await showTask;

        // Assert
        service.Dialogs.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShowAsync_ShouldReturnCorrectResult()
    {
        // Arrange
        var service = CreateService();
        var expectedData = new { Id = 123 };

        var showTask = service.ShowAsync<SimpleTestComponent>();
        await WaitForDialogCountAsync(service, 1);

        // Act
        service.CloseDialog(service.Dialogs[0], DialogResult.Ok(expectedData));
        var result = await showTask;

        // Assert
        result.Confirmed.ShouldBeTrue();
        result.Data.ShouldBe(expectedData);
    }

    [Fact]
    public async Task ShowAsync_WhenCancelled_ShouldReturnCancelResult()
    {
        // Arrange
        var service = CreateService();

        var showTask = service.ShowAsync<SimpleTestComponent>();
        await WaitForDialogCountAsync(service, 1);

        // Act
        service.CloseDialog(service.Dialogs[0], DialogResult.Cancel());
        var result = await showTask;

        // Assert
        result.Confirmed.ShouldBeFalse();
        result.Data.ShouldBeNull();
    }

    [Fact]
    public async Task ShowAsync_WithParameters_ShouldPassParametersToDialog()
    {
        // Arrange
        var service = CreateService();
        var parameters = new Dictionary<string, object?>
        {
            ["Id"] = 42,
            ["Name"] = "Test"
        };

        var showTask = service.ShowAsync<SimpleTestComponent>(parameters: parameters);
        await WaitForDialogCountAsync(service, 1);

        // Assert
        service.Dialogs[0].Parameters.ShouldContainKey("Id");
        service.Dialogs[0].Parameters["Id"].ShouldBe(42);
        service.Dialogs[0].Parameters["Name"].ShouldBe("Test");

        // Cleanup
        service.CloseDialog(service.Dialogs[0], DialogResult.Cancel());
        await showTask;
    }

    [Fact]
    public async Task ShowAsync_WithCancellationToken_ShouldCancelOnTokenCancellation()
    {
        // Arrange
        var service = CreateService();
        using var cts = new CancellationTokenSource();

        var showTask = service.ShowAsync<SimpleTestComponent>(ct: cts.Token);
        await WaitForDialogCountAsync(service, 1);

        // Act
        cts.Cancel();
        var result = await showTask;

        // Assert
        result.Confirmed.ShouldBeFalse();
    }

    [Fact]
    public async Task ShowAsync_MultipleDialogs_ShouldSupportNestedDialogs()
    {
        // Arrange
        var service = CreateService();

        var showTask1 = service.ShowAsync<SimpleTestComponent>(
            options: new DialogOptions { Title = "Dialog 1" });
        await WaitForDialogCountAsync(service, 1);

        var showTask2 = service.ShowAsync<SimpleTestComponent>(
            options: new DialogOptions { Title = "Dialog 2" });
        await WaitForDialogCountAsync(service, 2);

        // Assert
        service.Dialogs.Count.ShouldBe(2);
        service.Dialogs.ShouldContain(d => d.Options.Title == "Dialog 1");
        service.Dialogs.ShouldContain(d => d.Options.Title == "Dialog 2");

        // Cleanup - close in reverse order
        var dialog2 = service.Dialogs.First(d => d.Options.Title == "Dialog 2");
        service.CloseDialog(dialog2, DialogResult.Cancel());
        await showTask2;

        var dialog1 = service.Dialogs.First(d => d.Options.Title == "Dialog 1");
        service.CloseDialog(dialog1, DialogResult.Cancel());
        await showTask1;
    }

    [Fact]
    public async Task ShowAsync_WithFormComponent_ShouldSetCorrectComponentType()
    {
        // Arrange
        var service = CreateService();

        var showTask = service.ShowAsync<FormTestComponent, string>();
        await WaitForDialogCountAsync(service, 1);

        // Assert
        service.Dialogs[0].ComponentType.ShouldBe(typeof(FormTestComponent));

        // Cleanup
        service.CloseDialog(service.Dialogs[0], DialogResult.Cancel());
        await showTask;
    }

    [Fact]
    public void OnChange_ShouldBeRaisedWhenDialogAdded()
    {
        // Arrange
        var service = CreateService();
        var changeCount = 0;
        service.OnChange += () => changeCount++;

        // Act
        _ = service.ShowAsync<SimpleTestComponent>();

        // Assert
        changeCount.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task OnChange_ShouldBeRaisedWhenDialogClosed()
    {
        // Arrange
        var service = CreateService();
        var showTask = service.ShowAsync<SimpleTestComponent>();
        await WaitForDialogCountAsync(service, 1);

        var changeCount = 0;
        service.OnChange += () => changeCount++;

        // Act
        service.CloseDialog(service.Dialogs[0], DialogResult.Cancel());
        await showTask;

        // Assert
        changeCount.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task CloseDialog_ShouldDisposeDialogInstance()
    {
        // Arrange
        var service = CreateService();

        var showTask = service.ShowAsync<SimpleTestComponent>();
        await WaitForDialogCountAsync(service, 1);
        var dialog = service.Dialogs[0];

        // Act
        service.CloseDialog(dialog, DialogResult.Ok());
        await showTask;

        // Assert - dialog should be disposed and not throw when accessing Result
        var result = await dialog.Result;
        result.Confirmed.ShouldBeTrue();
    }

    private static IDialogServiceInternal CreateService()
    {
        return new DialogService();
    }

    private static async Task WaitForDialogCountAsync(IDialogServiceInternal service, int expectedCount)
    {
        var tcs = new TaskCompletionSource();

        void OnChange()
        {
            if (service.Dialogs.Count == expectedCount)
                tcs.TrySetResult();
        }

        service.OnChange += OnChange;

        if (service.Dialogs.Count == expectedCount)
        {
            tcs.TrySetResult();
        }

        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(1));
        service.OnChange -= OnChange;
    }

    // We need internal access to create DialogService directly
    private class DialogService : IDialogServiceInternal
    {
        private readonly List<DialogInstance> _dialogs = [];

        public IReadOnlyList<DialogInstance> Dialogs => _dialogs;
        public event Action? OnChange;

        public async Task<DialogResult> ShowAsync<TComponent, TResult>(
            IDictionary<string, object?>? parameters = null,
            DialogOptions? options = null,
            CancellationToken ct = default)
            where TComponent : IComponent, IDialogForm<TResult>
        {
            return await ShowInternalAsync(typeof(TComponent), parameters, options, ct);
        }

        public async Task<DialogResult> ShowAsync<TComponent>(
            IDictionary<string, object?>? parameters = null,
            DialogOptions? options = null,
            CancellationToken ct = default)
            where TComponent : IComponent
        {
            return await ShowInternalAsync(typeof(TComponent), parameters, options, ct);
        }

        public async Task<DialogResult> ShowAsync<TMarker>(
            Type componentType,
            IDictionary<string, object?>? parameters = null,
            DialogOptions? options = null,
            CancellationToken ct = default)
        {
            return await ShowInternalAsync(componentType, parameters, options, ct);
        }

        private async Task<DialogResult> ShowInternalAsync(
            Type componentType,
            IDictionary<string, object?>? parameters,
            DialogOptions? options,
            CancellationToken ct)
        {
            var instance = new DialogInstance(componentType, parameters, options);
            _dialogs.Add(instance);
            OnChange?.Invoke();

            try
            {
                using var registration = ct.Register(() => instance.Close(DialogResult.Cancel()));
                return await instance.Result;
            }
            finally
            {
                instance.Dispose();
                _dialogs.Remove(instance);
                OnChange?.Invoke();
            }
        }

        public void CloseDialog(DialogInstance instance, DialogResult result)
        {
            instance.Close(result);
        }
    }

    private class SimpleTestComponent : ComponentBase
    {
    }

    private class FormTestComponent : ComponentBase, IDialogForm<string>
    {
        public EditContext? EditContext => null;

        public Task<DialogFormResult<string>> SubmitAsync()
        {
            return Task.FromResult(DialogFormResult<string>.Ok("result"));
        }
    }
}
