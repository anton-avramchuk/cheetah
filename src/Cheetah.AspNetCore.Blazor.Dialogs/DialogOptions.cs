namespace Cheetah.AspNetCore.Blazor.Dialogs;

public record DialogOptions
{
    public string Title { get; init; } = string.Empty;
    public string? Message { get; init; }
    public Type? ContentType { get; init; }
    public Dictionary<string, object>? ContentParameters { get; init; }
    public DialogDimension? Width { get; init; }
    public DialogDimension? Height { get; init; }
    public IReadOnlyList<IDialogCommand> Commands { get; init; } = DialogCommands.OkCancel;
}
