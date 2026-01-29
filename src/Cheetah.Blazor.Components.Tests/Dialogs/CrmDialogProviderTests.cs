using Bunit;
using Cheetah.Blazor.Components.Dialogs;
using Cheetah.Blazor.Components.Tests.Dialogs.TestComponents;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Blazor.Components.Tests.Dialogs;

public class CrmDialogProviderTests : TestContext
{
    [Fact]
    public void InitialRender_ShouldNotRenderAnyDialogs()
    {
        // Arrange
        var service = new TestDialogService();
        Services.AddSingleton<IDialogService>(service);
        Services.AddSingleton<IDialogServiceInternal>(service);

        // Act
        var cut = RenderComponent<CrmDialogProvider>();

        // Assert
        cut.FindAll(".modal").Should().BeEmpty();
    }

    [Fact]
    public async Task WhenDialogAdded_ShouldRenderDialog()
    {
        // Arrange
        var service = new TestDialogService();
        Services.AddSingleton<IDialogService>(service);
        Services.AddSingleton<IDialogServiceInternal>(service);

        var cut = RenderComponent<CrmDialogProvider>();

        // Act
        var showTask = service.ShowAsync<SimpleDialogContent>(
            options: new DialogOptions { Title = "Test Dialog" });

        // Wait for render
        cut.WaitForState(() => cut.FindAll(".modal").Count == 1);

        // Assert
        cut.Find(".modal-title").TextContent.Should().Be("Test Dialog");

        // Cleanup
        service.CloseDialog(service.Dialogs[0], DialogResult.Cancel());
        await showTask;
    }

    [Fact]
    public async Task WhenDialogClosed_ShouldRemoveDialog()
    {
        // Arrange
        var service = new TestDialogService();
        Services.AddSingleton<IDialogService>(service);
        Services.AddSingleton<IDialogServiceInternal>(service);

        var cut = RenderComponent<CrmDialogProvider>();

        var showTask = service.ShowAsync<SimpleDialogContent>();
        cut.WaitForState(() => cut.FindAll(".modal").Count == 1);

        // Act
        service.CloseDialog(service.Dialogs[0], DialogResult.Ok());
        await showTask;

        // Wait for render
        cut.WaitForState(() => cut.FindAll(".modal").Count == 0);

        // Assert
        cut.FindAll(".modal").Should().BeEmpty();
    }

    [Fact]
    public async Task MultipleDialogs_ShouldRenderAll()
    {
        // Arrange
        var service = new TestDialogService();
        Services.AddSingleton<IDialogService>(service);
        Services.AddSingleton<IDialogServiceInternal>(service);

        var cut = RenderComponent<CrmDialogProvider>();

        // Act
        var showTask1 = service.ShowAsync<SimpleDialogContent>(
            options: new DialogOptions { Title = "Dialog 1" });
        cut.WaitForState(() => cut.FindAll(".modal").Count == 1);

        var showTask2 = service.ShowAsync<SimpleDialogContent>(
            options: new DialogOptions { Title = "Dialog 2" });
        cut.WaitForState(() => cut.FindAll(".modal").Count == 2);

        // Assert
        var modals = cut.FindAll(".modal");
        modals.Should().HaveCount(2);

        var titles = cut.FindAll(".modal-title").Select(e => e.TextContent).ToList();
        titles.Should().Contain("Dialog 1");
        titles.Should().Contain("Dialog 2");

        // Cleanup
        service.CloseDialog(service.Dialogs[1], DialogResult.Cancel());
        await showTask2;
        service.CloseDialog(service.Dialogs[0], DialogResult.Cancel());
        await showTask1;
    }

    [Fact]
    public async Task ClosingDialog_ShouldReturnCorrectResult()
    {
        // Arrange
        var service = new TestDialogService();
        Services.AddSingleton<IDialogService>(service);
        Services.AddSingleton<IDialogServiceInternal>(service);

        var cut = RenderComponent<CrmDialogProvider>();

        var showTask = service.ShowAsync<SimpleDialogContent>();
        cut.WaitForState(() => cut.FindAll(".modal").Count == 1);

        // Act - click cancel button
        var cancelButton = cut.FindAll(".modal-footer button")[0];
        cancelButton.Click();

        var result = await showTask;

        // Assert
        result.Confirmed.Should().BeFalse();
    }

    [Fact]
    public void Dispose_ShouldUnsubscribeFromService()
    {
        // Arrange
        var service = new TestDialogService();
        Services.AddSingleton<IDialogService>(service);
        Services.AddSingleton<IDialogServiceInternal>(service);

        var cut = RenderComponent<CrmDialogProvider>();
        service.OnChangeSubscriberCount.Should().Be(1); // Verify subscription happened

        // Act
        DisposeComponents();

        // Assert
        service.OnChangeSubscriberCount.Should().Be(0);
    }

    private class TestDialogService : IDialogServiceInternal
    {
        private readonly List<DialogInstance> _dialogs = [];
        private Action? _onChange;

        public IReadOnlyList<DialogInstance> Dialogs => _dialogs;

        public int OnChangeSubscriberCount { get; private set; }

        public event Action? OnChange
        {
            add
            {
                _onChange += value;
                OnChangeSubscriberCount++;
            }
            remove
            {
                _onChange -= value;
                OnChangeSubscriberCount--;
            }
        }

        public async Task<DialogResult> ShowAsync<TComponent, TResult>(
            IDictionary<string, object?>? parameters = null,
            DialogOptions? options = null,
            CancellationToken ct = default)
            where TComponent : Microsoft.AspNetCore.Components.IComponent, IDialogForm<TResult>
        {
            return await ShowInternalAsync(typeof(TComponent), parameters, options, ct);
        }

        public async Task<DialogResult> ShowAsync<TComponent>(
            IDictionary<string, object?>? parameters = null,
            DialogOptions? options = null,
            CancellationToken ct = default)
            where TComponent : Microsoft.AspNetCore.Components.IComponent
        {
            return await ShowInternalAsync(typeof(TComponent), parameters, options, ct);
        }

        private async Task<DialogResult> ShowInternalAsync(
            Type componentType,
            IDictionary<string, object?>? parameters,
            DialogOptions? options,
            CancellationToken ct)
        {
            var instance = new DialogInstance(componentType, parameters, options);
            _dialogs.Add(instance);
            _onChange?.Invoke();

            try
            {
                using var registration = ct.Register(() => instance.Close(DialogResult.Cancel()));
                return await instance.Result;
            }
            finally
            {
                instance.Dispose();
                _dialogs.Remove(instance);
                _onChange?.Invoke();
            }
        }

        public void CloseDialog(DialogInstance instance, DialogResult result)
        {
            instance.Close(result);
        }
    }
}
