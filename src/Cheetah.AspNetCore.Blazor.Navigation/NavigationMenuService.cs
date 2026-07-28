using Cheetah.Core.DependencyInjection;
using Cheetah.AspNetCore.Blazor.Navigation.Builders;
using Cheetah.AspNetCore.Blazor.Navigation.Models;

namespace Cheetah.AspNetCore.Blazor.Navigation;

/// <summary>
/// Собирает меню из контрибьюторов. Scoped, а не Singleton: контрибьютор может брать пункты из
/// данных текущего пользователя (список его вакансий, избранное) — для этого ему нужны scoped-
/// зависимости, недоступные из корневого контейнера. Меню строится заново на каждый вызов.
/// </summary>
[Export(LifetimeType.Scoped, typeof(INavigationMenuService))]
public class NavigationMenuService : INavigationMenuService
{
    private readonly IMenuContributorProvider _contributorProvider;

    public NavigationMenuService(IMenuContributorProvider contributorProvider)
    {
        _contributorProvider = contributorProvider;
    }

    public async Task<Menu> GetMenuAsync(string menuId)
    {
        var builder = new MenuBuilder(menuId);
        foreach (var contributor in _contributorProvider.GetContributors(menuId))
            await contributor.ConfigureMenuAsync(builder);
        return builder.Build();
    }
}
