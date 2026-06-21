namespace Cheetah.AspNetCore.Blazor.Dialogs;

public sealed class ActiveDialog
{
    public DialogOptions Options { get; init; } = null!;
    public TaskCompletionSource<DialogResult> Tcs { get; } = new();
    public DialogFormContext FormContext { get; } = new();

    public string? Error { get; private set; }
    public bool IsBusy { get; private set; }

    public event Action? OnStateChanged;

    public void SetError(string? error)
    {
        if (Error == error)
            return;
        Error = error;
        OnStateChanged?.Invoke();
    }

    public void SetBusy(bool busy)
    {
        if (IsBusy == busy)
            return;
        IsBusy = busy;
        OnStateChanged?.Invoke();
    }
}
