namespace Cheetah.AspNetCore.Blazor.Dialogs;

public interface IDialogContext
{
    Task CloseAsync(string commandId);

    /// <summary>
    /// Shows an inline error message inside the dialog without closing it.
    /// Pass <c>null</c> to clear a previously shown error.
    /// </summary>
    void ShowError(string? message);
}
