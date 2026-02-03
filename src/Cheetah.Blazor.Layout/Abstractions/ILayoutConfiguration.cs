namespace Cheetah.Blazor.Layout.Abstractions;

/// <summary>
/// Configuration options for the CRM layout.
/// </summary>
public interface ILayoutConfiguration
{
    /// <summary>
    /// Application name displayed in the header.
    /// </summary>
    string ApplicationName { get; }

    /// <summary>
    /// URL or path to the application logo.
    /// </summary>
    string? LogoUrl { get; }

    /// <summary>
    /// URL to navigate when clicking the logo/app name.
    /// </summary>
    string HomeUrl { get; }

    /// <summary>
    /// Whether the sidebar is collapsed by default.
    /// </summary>
    bool SidebarCollapsedByDefault { get; }

    /// <summary>
    /// Primary color for the theme (CSS variable).
    /// </summary>
    string PrimaryColor { get; }

    /// <summary>
    /// Name of the main menu to display in sidebar.
    /// </summary>
    string MainMenuName { get; }
}
