using Cheetah.Blazor.Components.Icons;
using Cheetah.Frontend.Navigation.Models;

namespace Cheetah.Frontend.Navigation.Extensions;

/// <summary>
/// Extension methods for menu configuration.
/// </summary>
public static class MenuExtensions
{
    /// <summary>
    /// Adds a menu item to the menu with icon support.
    /// </summary>
    public static Menu AddItem(
        this Menu menu,
        string id,
        string name,
        string? iconName = null,
        IconSet iconSet = IconSet.Bootstrap,
        string? url = null,
        string? permission = null,
        int order = 0,
        Action<MenuItem>? configure = null)
    {
        var item = new MenuItem
        {
            Id = id,
            Name = name,
            IconName = iconName,
            IconSet = iconSet,
            Url = url,
            Permission = permission,
            Order = order
        };

        configure?.Invoke(item);
        menu.Items.Add(item);
        return menu;
    }

    /// <summary>
    /// Adds a child menu item with icon support.
    /// </summary>
    public static MenuItem AddChild(
        this MenuItem parent,
        string id,
        string name,
        string? iconName = null,
        IconSet iconSet = IconSet.Bootstrap,
        string? url = null,
        string? permission = null,
        int order = 0,
        Action<MenuItem>? configure = null)
    {
        var item = new MenuItem
        {
            Id = id,
            Name = name,
            IconName = iconName,
            IconSet = iconSet,
            Url = url,
            Permission = permission,
            Order = order
        };

        configure?.Invoke(item);
        parent.Children.Add(item);
        return parent;
    }

    /// <summary>
    /// Sets the icon for a menu item.
    /// </summary>
    public static MenuItem WithIcon(this MenuItem item, string iconName, IconSet iconSet = IconSet.Bootstrap)
    {
        item.IconName = iconName;
        item.IconSet = iconSet;
        return item;
    }

    /// <summary>
    /// Sets custom data on a menu item.
    /// </summary>
    public static MenuItem WithCustomData(this MenuItem item, string key, object value)
    {
        item.CustomData[key] = value;
        return item;
    }

    /// <summary>
    /// Sets the menu item as disabled.
    /// </summary>
    public static MenuItem AsDisabled(this MenuItem item, bool disabled = true)
    {
        item.IsDisabled = disabled;
        return item;
    }

    /// <summary>
    /// Sets the menu item as hidden.
    /// </summary>
    public static MenuItem AsHidden(this MenuItem item, bool hidden = true)
    {
        item.IsVisible = !hidden;
        return item;
    }

    /// <summary>
    /// Opens link in a new tab.
    /// </summary>
    public static MenuItem OpenInNewTab(this MenuItem item)
    {
        item.Target = "_blank";
        return item;
    }

    /// <summary>
    /// Filters visible menu items based on a predicate.
    /// </summary>
    public static IEnumerable<MenuItem> WhereVisible(this IEnumerable<MenuItem> items)
    {
        return items.Where(i => i.IsVisible);
    }

    /// <summary>
    /// Creates a deep copy of the menu with filtered items based on permissions.
    /// </summary>
    public static Menu FilterByPermissions(this Menu menu, Func<string?, bool> hasPermission)
    {
        var filteredMenu = new Menu(menu.Name);

        foreach (var item in menu.Items)
        {
            var filteredItem = FilterItemRecursive(item, hasPermission);
            if (filteredItem != null)
            {
                filteredMenu.Items.Add(filteredItem);
            }
        }

        return filteredMenu;
    }

    private static MenuItem? FilterItemRecursive(MenuItem item, Func<string?, bool> hasPermission)
    {
        // Check if user has permission for this item
        if (!string.IsNullOrEmpty(item.Permission) && !hasPermission(item.Permission))
        {
            return null;
        }

        if (!item.IsVisible)
        {
            return null;
        }

        // Create a copy
        var copy = new MenuItem
        {
            Id = item.Id,
            Name = item.Name,
            IconName = item.IconName,
            IconSet = item.IconSet,
#pragma warning disable CS0618 // Type or member is obsolete
            Icon = item.Icon,
#pragma warning restore CS0618
            Url = item.Url,
            Permission = item.Permission,
            Order = item.Order,
            IsVisible = item.IsVisible,
            IsDisabled = item.IsDisabled,
            Target = item.Target,
            CssClass = item.CssClass,
            CustomData = new Dictionary<string, object>(item.CustomData)
        };

        // Filter children
        foreach (var child in item.Children)
        {
            var filteredChild = FilterItemRecursive(child, hasPermission);
            if (filteredChild != null)
            {
                copy.Children.Add(filteredChild);
            }
        }

        // If this is a parent item with no URL and no visible children, hide it
        if (string.IsNullOrEmpty(copy.Url) && copy.Children.Count == 0)
        {
            return null;
        }

        return copy;
    }
}
