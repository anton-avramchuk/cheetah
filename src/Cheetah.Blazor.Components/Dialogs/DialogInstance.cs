using Microsoft.AspNetCore.Components.Forms;

namespace Cheetah.Blazor.Components.Dialogs;

/// <summary>
/// Represents a single dialog instance.
/// Manages state, validation tracking, and result completion.
/// </summary>
public sealed class DialogInstance : IDialogFormContext, IDisposable
{
    private readonly TaskCompletionSource<DialogResult> _tcs = new();
    private EditContext? _editContext;
    private bool _disposed;

    public Guid Id { get; } = Guid.NewGuid();
    public Type ComponentType { get; }
    public IDictionary<string, object?> Parameters { get; }
    public DialogOptions Options { get; }

    public Task<DialogResult> Result => _tcs.Task;
    public bool IsFormValid => _editContext?.Validate() ?? true;

    public event Action? OnStateChanged;

    public DialogInstance(Type componentType, IDictionary<string, object?>? parameters, DialogOptions? options)
    {
        ComponentType = componentType;
        Parameters = parameters ?? new Dictionary<string, object?>();
        Options = options ?? new DialogOptions();
    }

    public void RegisterEditContext(EditContext? editContext)
    {
        if (_editContext != null)
        {
            _editContext.OnValidationStateChanged -= HandleValidationChanged;
        }

        _editContext = editContext;

        if (_editContext != null)
        {
            _editContext.OnValidationStateChanged += HandleValidationChanged;
        }

        OnStateChanged?.Invoke();
    }

    private void HandleValidationChanged(object? sender, ValidationStateChangedEventArgs e)
    {
        OnStateChanged?.Invoke();
    }

    public void Close(DialogResult result)
    {
        _tcs.TrySetResult(result);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_editContext != null)
        {
            _editContext.OnValidationStateChanged -= HandleValidationChanged;
        }
    }
}
