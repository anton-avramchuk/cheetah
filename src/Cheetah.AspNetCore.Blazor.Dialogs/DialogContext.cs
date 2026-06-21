namespace Cheetah.AspNetCore.Blazor.Dialogs;

internal sealed class DialogContext(IDialogHost host, ActiveDialog dialog) : IDialogContext
{
    public Task CloseAsync(string commandId)
    {
        host.Close(dialog, commandId);
        return Task.CompletedTask;
    }

    public void ShowError(string? message) => dialog.SetError(message);
}
