using Cheetah.Blazor.Layout.Abstractions;

namespace Cheetah.Blazor.Layout.Configuration;

/// <summary>
/// Default implementation of layout configuration.
/// </summary>
public class LayoutConfiguration : ILayoutConfiguration
{
    public string ApplicationName { get; set; } = "Cheetah CRM";
    public string? LogoUrl { get; set; }
    public string HomeUrl { get; set; } = "/";
    public bool SidebarCollapsedByDefault { get; set; } = false;
    public string PrimaryColor { get; set; } = "#0d6efd";
    public string MainMenuName { get; set; } = "Main";
}
