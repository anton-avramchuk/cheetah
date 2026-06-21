namespace Cheetah.AspNetCore.Blazor.Dialogs;

public abstract record DialogDimension
{
    public sealed record Pixels(int Value) : DialogDimension;
    public sealed record Percent(int Value) : DialogDimension;

    internal string ToCss() => this switch
    {
        Pixels p  => $"{p.Value}px",
        Percent p => $"{p.Value}%",
        _         => "auto"
    };
}
