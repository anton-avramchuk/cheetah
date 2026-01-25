using Cheetah.Frontend.Navigation.Models;

namespace Cheetah.Frontend.Navigation.Services.Abstractions;

/// <summary>
/// Context passed to menu contributors during menu configuration.
/// </summary>
public class MenuConfigurationContext
{
    private readonly Dictionary<string, Menu> _menus = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Service provider for resolving dependencies.
    /// </summary>
    public IServiceProvider ServiceProvider { get; }

    public MenuConfigurationContext(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    /// <summary>
    /// Gets an existing menu or creates a new one with the specified name.
    /// </summary>
    /// <param name="menuName">The name of the menu.</param>
    /// <returns>The menu instance.</returns>
    public Menu GetOrCreateMenu(string menuName)
    {
        if (!_menus.TryGetValue(menuName, out var menu))
        {
            menu = new Menu(menuName);
            _menus[menuName] = menu;
        }

        return menu;
    }

    /// <summary>
    /// Gets a menu by name. Returns null if not found.
    /// </summary>
    public Menu? GetMenu(string menuName)
    {
        return _menus.GetValueOrDefault(menuName);
    }

    /// <summary>
    /// Gets all configured menus.
    /// </summary>
    internal IReadOnlyDictionary<string, Menu> GetAllMenus() => _menus;
}
