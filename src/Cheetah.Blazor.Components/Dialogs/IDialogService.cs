using Cheetah.Core.DependencyInjection;
using Microsoft.AspNetCore.Components;

namespace Cheetah.Blazor.Components.Dialogs;

/// <summary>
/// Service for programmatically showing dialog windows with typed results.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Shows a dialog with the specified component type.
    /// </summary>
    /// <typeparam name="TComponent">Type of component to render in the dialog.</typeparam>
    /// <typeparam name="TResult">Type of result returned by the form.</typeparam>
    /// <param name="parameters">Parameters to pass to the component.</param>
    /// <param name="options">Dialog options for appearance and behavior.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Dialog result containing confirmation status and data.</returns>
    Task<DialogResult> ShowAsync<TComponent, TResult>(
        IDictionary<string, object?>? parameters = null,
        DialogOptions? options = null,
        CancellationToken ct = default)
        where TComponent : IComponent, IDialogForm<TResult>;

    /// <summary>
    /// Shows a simple dialog without form validation.
    /// </summary>
    /// <typeparam name="TComponent">Type of component to render in the dialog.</typeparam>
    /// <param name="parameters">Parameters to pass to the component.</param>
    /// <param name="options">Dialog options for appearance and behavior.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Dialog result containing confirmation status.</returns>
    Task<DialogResult> ShowAsync<TComponent>(
        IDictionary<string, object?>? parameters = null,
        DialogOptions? options = null,
        CancellationToken ct = default)
        where TComponent : IComponent;
}

/// <summary>
/// Internal interface for DialogProvider to access dialog instances.
/// </summary>
internal interface IDialogServiceInternal : IDialogService
{
    IReadOnlyList<DialogInstance> Dialogs { get; }
    event Action? OnChange;
    void CloseDialog(DialogInstance instance, DialogResult result);
}

[Export(LifetimeType.Scoped, typeof(IDialogService), typeof(IDialogServiceInternal))]
internal sealed class DialogService : IDialogServiceInternal
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
