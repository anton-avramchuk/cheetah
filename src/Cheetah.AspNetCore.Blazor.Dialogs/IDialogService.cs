namespace Cheetah.AspNetCore.Blazor.Dialogs;

public interface IDialogService
{
    Task AlertAsync(string message, string title = "");
    Task<bool> ConfirmAsync(string message, string title = "Подтверждение");
    Task<DialogResult> ShowAsync(DialogOptions options);
}
