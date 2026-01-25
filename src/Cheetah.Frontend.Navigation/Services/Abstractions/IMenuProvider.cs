using Cheetah.Frontend.Navigation.Models;

namespace Cheetah.Frontend.Navigation.Services.Abstractions;

/// <summary>
/// Provides access to application menus.
/// </summary>
public interface IMenuProvider
{
    /// <summary>
    /// Gets a menu by name.
    /// </summary>
    /// <param name="name">The menu name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The menu, or null if not found.</returns>
    ValueTask<Menu?> GetMenuAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all available menus.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of all menus.</returns>
    ValueTask<IReadOnlyList<Menu>> GetAllMenusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Forces a refresh of the menu cache.
    /// Call this when menu configuration may have changed (e.g., permissions changed).
    /// </summary>
    void Invalidate();
}
