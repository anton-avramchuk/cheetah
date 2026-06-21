namespace Cheetah.AspNetCore.Blazor.Dialogs;

public interface IDialogHost
{
    event Action<IReadOnlyList<ActiveDialog>>? OnDialogChanged;
    void Close(ActiveDialog dialog, string commandId);
}
