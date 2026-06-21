using Cheetah.Core.DependencyInjection;

namespace Cheetah.AspNetCore.Blazor.Dialogs;

[Export(LifetimeType.Scoped, typeof(IDialogService), typeof(IDialogHost))]
public class DialogService : IDialogService, IDialogHost
{
    internal event Action<IReadOnlyList<ActiveDialog>>? OnDialogChanged;

    event Action<IReadOnlyList<ActiveDialog>>? IDialogHost.OnDialogChanged
    {
        add    => OnDialogChanged += value;
        remove => OnDialogChanged -= value;
    }

    void IDialogHost.Close(ActiveDialog dialog, string commandId) => Close(dialog, commandId);

    private readonly List<ActiveDialog> _stack = new();

    public Task AlertAsync(string message, string title = "")
        => ShowAsync(new DialogOptions
        {
            Title    = title,
            Message  = message,
            Commands = DialogCommands.OkOnly
        });

    public async Task<bool> ConfirmAsync(string message, string title = "Подтверждение")
    {
        var result = await ShowAsync(new DialogOptions
        {
            Title    = title,
            Message  = message,
            Commands = DialogCommands.OkCancel
        });
        return result.IsOk;
    }

    public Task<DialogResult> ShowAsync(DialogOptions options)
    {
        var dialog = new ActiveDialog { Options = options };
        _stack.Add(dialog);
        Notify();
        return dialog.Tcs.Task;
    }

    internal void Close(ActiveDialog dialog, string commandId)
    {
        _stack.Remove(dialog);
        Notify();
        dialog.Tcs.SetResult(new DialogResult(commandId));
    }

    private void Notify() => OnDialogChanged?.Invoke(_stack.AsReadOnly());
}
