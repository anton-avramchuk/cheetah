namespace Cheetah.AspNetCore.Blazor.Dialogs;

public static class DialogCommands
{
    public static readonly IDialogCommand Ok     = new DialogCommand("ok",     "ОК",      IsPrimary: true);
    public static readonly IDialogCommand Cancel = new DialogCommand("cancel", "Отмена",  IsCancel: true);

    public static readonly IReadOnlyList<IDialogCommand> OkOnly    = [Ok];
    public static readonly IReadOnlyList<IDialogCommand> OkCancel  = [Ok, Cancel];
}
