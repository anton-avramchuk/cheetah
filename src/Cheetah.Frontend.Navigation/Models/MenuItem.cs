using Cheetah.Blazor.Components.Icons;

namespace Cheetah.Frontend.Navigation.Models;

/// <summary>
/// Represents a menu item in the navigation system.
/// </summary>
public class MenuItem
{
    /// <summary>
    /// Unique identifier for the menu item.
    /// </summary>
    public string Id { get; set; } = null!;

    /// <summary>
    /// Display name of the menu item.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Icon name (e.g., "house", "people", "settings").
    /// Use with <see cref="IconSet"/> to specify the icon set.
    /// </summary>
    public string? IconName { get; set; }

    /// <summary>
    /// Icon set to use. Defaults to Bootstrap.
    /// </summary>
    public IconSet IconSet { get; set; } = IconSet.Bootstrap;

    /// <summary>
    /// Legacy icon CSS class. Deprecated - use <see cref="IconName"/> and <see cref="IconSet"/> instead.
    /// </summary>
    [Obsolete("Use IconName and IconSet instead")]
    public string? Icon { get; set; }

    /// <summary>
    /// Navigation URL. Null for parent items with children.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Required permission to see this menu item.
    /// </summary>
    public string? Permission { get; set; }

    /// <summary>
    /// Sort order within the parent.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Whether the menu item is visible.
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Whether the menu item is disabled (visible but not clickable).
    /// </summary>
    public bool IsDisabled { get; set; }

    /// <summary>
    /// Link target (_blank, _self, etc.).
    /// </summary>
    public string? Target { get; set; }

    /// <summary>
    /// CSS class to apply to the menu item.
    /// </summary>
    public string? CssClass { get; set; }

    /// <summary>
    /// Custom data for extensibility.
    /// </summary>
    public Dictionary<string, object> CustomData { get; set; } = [];

    /// <summary>
    /// Child menu items.
    /// </summary>
    public List<MenuItem> Children { get; set; } = [];

    /// <summary>
    /// Adds a child menu item.
    /// </summary>
    public MenuItem AddChild(MenuItem child)
    {
        Children.Add(child);
        return this;
    }

    /// <summary>
    /// Finds a menu item by ID in this item and its descendants.
    /// </summary>
    public MenuItem? FindById(string id)
    {
        if (Id == id)
            return this;

        foreach (var child in Children)
        {
            var found = child.FindById(id);
            if (found != null)
                return found;
        }

        return null;
    }

    /// <summary>
    /// Gets all menu items in a flat list (including this item and all descendants).
    /// </summary>
    public IEnumerable<MenuItem> GetAllItems()
    {
        yield return this;

        foreach (var child in Children)
        {
            foreach (var item in child.GetAllItems())
            {
                yield return item;
            }
        }
    }
}
