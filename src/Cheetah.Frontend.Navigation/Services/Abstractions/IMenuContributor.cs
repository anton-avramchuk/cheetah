namespace Cheetah.Frontend.Navigation.Services.Abstractions;

/// <summary>
/// Interface for modules to contribute menu items.
/// Implement this interface in your module to add menu items to the navigation system.
/// </summary>
public interface IMenuContributor
{
    /// <summary>
    /// Order of execution. Lower values execute first.
    /// Default is 0. Use negative values for high priority, positive for lower priority.
    /// </summary>
    int Order => 0;

    /// <summary>
    /// Configures menu items for this contributor.
    /// </summary>
    /// <param name="context">The menu configuration context.</param>
    Task ConfigureMenuAsync(MenuConfigurationContext context);
}
