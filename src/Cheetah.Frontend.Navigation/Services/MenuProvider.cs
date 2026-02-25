using Cheetah.Core.DependencyInjection;
using Cheetah.Frontend.Navigation.Extensions;
using Cheetah.Frontend.Navigation.Models;
using Cheetah.Frontend.Navigation.Services.Abstractions;
using Microsoft.AspNetCore.Components.Authorization;

namespace Cheetah.Frontend.Navigation.Services;

/// <summary>
/// Aggregates menu items from all registered <see cref="IMenuContributor"/> implementations
/// and filters them by the current user's roles.
/// Registered as Scoped so each user session gets its own filtered view.
/// The full menu structure (from contributors) is built once and cached;
/// the role-filtered result is invalidated automatically on every auth state change.
/// </summary>
[Export(LifetimeType.Scoped, typeof(IMenuProvider))]
public class MenuProvider : IMenuProvider, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEnumerable<IMenuContributor> _contributors;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    // Full unfiltered structure — built once from contributors
    private Dictionary<string, Menu>? _rawMenus;
    // Role-filtered result — invalidated on every auth state change
    private volatile Dictionary<string, Menu>? _filteredMenus;

    public MenuProvider(
        IServiceProvider serviceProvider,
        IEnumerable<IMenuContributor> contributors,
        AuthenticationStateProvider authStateProvider)
    {
        _serviceProvider = serviceProvider;
        _contributors = contributors;
        _authStateProvider = authStateProvider;

        // Re-filter whenever the user logs in or out
        _authStateProvider.AuthenticationStateChanged += OnAuthStateChanged;
    }

    public async ValueTask<Menu?> GetMenuAsync(string name, CancellationToken ct = default)
    {
        var menus = await GetFilteredMenusAsync(ct);
        return menus.GetValueOrDefault(name);
    }

    public async ValueTask<IReadOnlyList<Menu>> GetAllMenusAsync(CancellationToken ct = default)
    {
        var menus = await GetFilteredMenusAsync(ct);
        return menus.Values.ToList();
    }

    public void Invalidate() => _filteredMenus = null;

    // --- internals ---

    private void OnAuthStateChanged(Task<AuthenticationState> _) => Invalidate();

    private async ValueTask<Dictionary<string, Menu>> GetFilteredMenusAsync(CancellationToken ct)
    {
        if (_filteredMenus != null)
            return _filteredMenus;

        await _initLock.WaitAsync(ct);
        try
        {
            if (_filteredMenus != null)
                return _filteredMenus;

            var raw = await GetRawMenusAsync(ct);

            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            var filtered = new Dictionary<string, Menu>(StringComparer.OrdinalIgnoreCase);
            foreach (var (name, menu) in raw)
                filtered[name] = menu.FilterByUser(user);

            _filteredMenus = filtered;
            return _filteredMenus;
        }
        finally
        {
            _initLock.Release();
        }
    }

    private async Task<Dictionary<string, Menu>> GetRawMenusAsync(CancellationToken ct)
    {
        // Called only from within _initLock, but double-check for clarity
        if (_rawMenus != null)
            return _rawMenus;

        var context = new MenuConfigurationContext(_serviceProvider);

        foreach (var contributor in _contributors.OrderBy(c => c.Order))
            await contributor.ConfigureMenuAsync(context);

        var all = context.GetAllMenus();
        foreach (var menu in all.Values)
            menu.SortItems();

        _rawMenus = new Dictionary<string, Menu>(all, StringComparer.OrdinalIgnoreCase);
        return _rawMenus;
    }

    public void Dispose()
    {
        _authStateProvider.AuthenticationStateChanged -= OnAuthStateChanged;
    }
}
