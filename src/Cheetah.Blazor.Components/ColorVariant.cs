namespace Cheetah.Blazor.Components;

/// <summary>
/// Bootstrap color variants for components
/// </summary>
public enum ColorVariant
{
    Primary,
    Secondary,
    Success,
    Danger,
    Warning,
    Info,
    Light,
    Dark
}

/// <summary>
/// Extension methods for ColorVariant
/// </summary>
public static class ColorVariantExtensions
{
    /// <summary>
    /// Get Bootstrap CSS class name for the color variant
    /// </summary>
    public static string ToCssClass(this ColorVariant variant) => variant switch
    {
        ColorVariant.Primary => "primary",
        ColorVariant.Secondary => "secondary",
        ColorVariant.Success => "success",
        ColorVariant.Danger => "danger",
        ColorVariant.Warning => "warning",
        ColorVariant.Info => "info",
        ColorVariant.Light => "light",
        ColorVariant.Dark => "dark",
        _ => "primary"
    };
}
