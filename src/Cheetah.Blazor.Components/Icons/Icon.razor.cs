using Microsoft.AspNetCore.Components;

namespace Cheetah.Blazor.Components.Icons;

public partial class Icon : ComponentBase
{
    /// <summary>
    /// The name of the icon. For Bootstrap icons, use names like "check", "house".
    /// For Material Symbols, use names like "home", "settings".
    /// </summary>
    [Parameter, EditorRequired]
    public string Name { get; set; } = "";

    /// <summary>
    /// The icon set to use. Defaults to Bootstrap.
    /// </summary>
    [Parameter]
    public IconSet Set { get; set; } = IconSet.Bootstrap;

    /// <summary>
    /// The size of the icon. Defaults to Medium (24px).
    /// </summary>
    [Parameter]
    public IconSize Size { get; set; } = IconSize.Medium;

    /// <summary>
    /// Optional CSS color value for the icon.
    /// </summary>
    [Parameter]
    public string? Color { get; set; }

    /// <summary>
    /// When true, applies a spinning animation to the icon.
    /// Useful for loading indicators.
    /// </summary>
    [Parameter]
    public bool Spin { get; set; }

    /// <summary>
    /// Additional CSS classes to apply to the icon.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Accessible title for the icon. When set, adds aria-label.
    /// </summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>
    /// Additional HTML attributes to apply to the icon element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private string GetCssClass()
    {
        var classes = new List<string>();

        // Icon set class
        switch (Set)
        {
            case IconSet.Bootstrap:
                classes.Add("bi");
                classes.Add($"bi-{Name}");
                break;
            case IconSet.Material:
                classes.Add("material-symbols-outlined");
                break;
            case IconSet.MaterialRounded:
                classes.Add("material-symbols-rounded");
                break;
            case IconSet.MaterialSharp:
                classes.Add("material-symbols-sharp");
                break;
        }

        // Size class
        classes.Add(Size switch
        {
            IconSize.Small => "cheetah-icon-sm",
            IconSize.Medium => "cheetah-icon-md",
            IconSize.Large => "cheetah-icon-lg",
            IconSize.XLarge => "cheetah-icon-xl",
            _ => "cheetah-icon-md"
        });

        // Spin animation
        if (Spin)
        {
            classes.Add("cheetah-icon-spin");
        }

        // Additional classes
        if (!string.IsNullOrWhiteSpace(Class))
        {
            classes.Add(Class);
        }

        return string.Join(" ", classes);
    }

    private string? GetStyle()
    {
        if (string.IsNullOrWhiteSpace(Color))
        {
            return null;
        }

        return $"color: {Color};";
    }
}
