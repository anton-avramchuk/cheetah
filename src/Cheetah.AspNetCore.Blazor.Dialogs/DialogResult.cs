namespace Cheetah.AspNetCore.Blazor.Dialogs;

public record DialogResult(string CommandId)
{
    public bool IsOk       => CommandId == DialogCommands.Ok.Id;
    public bool IsCancelled => CommandId == DialogCommands.Cancel.Id;
}
