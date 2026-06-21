namespace Cheetah.AspNetCore.Blazor.Dialogs;

public record DialogCommand(
    string Id,
    string Label,
    bool IsPrimary = false,
    bool IsCancel = false,
    Func<IDialogContext, Task>? OnExecute = null) : IDialogCommand
{
    public Task ExecuteAsync(IDialogContext context)
        => OnExecute is not null ? OnExecute(context) : context.CloseAsync(Id);
}
