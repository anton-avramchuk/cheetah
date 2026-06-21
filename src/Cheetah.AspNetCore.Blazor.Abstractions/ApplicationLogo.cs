namespace Cheetah.AspNetCore.Blazor.Abstractions;

public abstract record ApplicationLogo
{
    public sealed record Icon(string CssClass) : ApplicationLogo;
    public sealed record Image(string Src, string? Alt = null) : ApplicationLogo;
}
