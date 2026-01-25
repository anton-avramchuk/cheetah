namespace Cheetah.Frontend.Navigation.Models;

/// <summary>
/// Represents a named menu containing menu items.
/// </summary>
public class Menu
{
    /// <summary>
    /// Unique name of the menu.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Root menu items.
    /// </summary>
    public List<MenuItem> Items { get; } = [];

    public Menu(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }

    /// <summary>
    /// Adds a menu item to the root level.
    /// </summary>
    public Menu AddItem(MenuItem item)
    {
        Items.Add(item);
        return this;
    }

    /// <summary>
    /// Finds a menu item by ID in the entire menu tree.
    /// </summary>
    public MenuItem? FindById(string id)
    {
        foreach (var item in Items)
        {
            var found = item.FindById(id);
            if (found != null)
                return found;
        }

        return null;
    }

    /// <summary>
    /// Gets or creates a menu item by ID. Creates parent hierarchy if needed.
    /// </summary>
    /// <param name="id">Menu item ID (can be hierarchical like "Admin.Users")</param>
    /// <param name="configure">Optional configuration action for newly created items</param>
    public MenuItem GetOrAdd(string id, Action<MenuItem>? configure = null)
    {
        var existing = FindById(id);
        if (existing != null)
            return existing;

        var newItem = new MenuItem { Id = id, Name = id };
        configure?.Invoke(newItem);
        Items.Add(newItem);
        return newItem;
    }

    /// <summary>
    /// Gets all menu items in a flat list.
    /// </summary>
    public IEnumerable<MenuItem> GetAllItems()
    {
        foreach (var item in Items)
        {
            foreach (var descendant in item.GetAllItems())
            {
                yield return descendant;
            }
        }
    }

    /// <summary>
    /// Sorts all items recursively by Order property.
    /// </summary>
    public void SortItems()
    {
        SortItemsRecursive(Items);
    }

    private static void SortItemsRecursive(List<MenuItem> items)
    {
        items.Sort((a, b) => a.Order.CompareTo(b.Order));

        foreach (var item in items)
        {
            if (item.Children.Count > 0)
            {
                SortItemsRecursive(item.Children);
            }
        }
    }
}
