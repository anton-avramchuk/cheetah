namespace Cheetah.AspNetCore.Blazor.Dialogs;

public interface IDialogCommand
{
    string Id { get; }
    string Label { get; }
    bool IsPrimary { get; }
    bool IsCancel { get; }
    Task ExecuteAsync(IDialogContext context);
}
