using Cheetah.Core.DependencyInjection;
using Cheetah.Frontend.Navigation.Models;
using Cheetah.Frontend.Navigation.Services.Abstractions;

namespace Cheetah.Frontend.Navigation.Services;

/// <summary>
/// Default implementation of <see cref="IMenuProvider"/> that aggregates menu items
/// from all registered <see cref="IMenuContributor"/> implementations.
/// </summary>
[Export(LifetimeType.Singleton, typeof(IMenuProvider))]
public class MenuProvider : IMenuProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEnumerable<IMenuContributor> _contributors;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    private volatile Dictionary<string, Menu>? _menus;

    public MenuProvider(
        IServiceProvider serviceProvider,
        IEnumerable<IMenuContributor> contributors)
    {
        _serviceProvider = serviceProvider;
        _contributors = contributors;
    }

    /// <inheritdoc />
    public async ValueTask<Menu?> GetMenuAsync(string name, CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);
        return _menus!.GetValueOrDefault(name);
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<Menu>> GetAllMenusAsync(CancellationToken cancellationToken = default)
    {
        await EnsureInitializedAsync(cancellationToken);
        return _menus!.Values.ToList();
    }

    /// <inheritdoc />
    public void Invalidate()
    {
        _menus = null;
    }

    private async Task EnsureInitializedAsync(CancellationToken cancellationToken)
    {
        if (_menus != null)
            return;

        await _initLock.WaitAsync(cancellationToken);
        try
        {
            if (_menus != null)
                return;

            var context = new MenuConfigurationContext(_serviceProvider);

            // Execute contributors in order
            var orderedContributors = _contributors.OrderBy(c => c.Order);

            foreach (var contributor in orderedContributors)
            {
                await contributor.ConfigureMenuAsync(context);
            }

            // Sort items in all menus
            var menus = context.GetAllMenus();
            foreach (var menu in menus.Values)
            {
                menu.SortItems();
            }

            _menus = new Dictionary<string, Menu>(menus, StringComparer.OrdinalIgnoreCase);
        }
        finally
        {
            _initLock.Release();
        }
    }
}
