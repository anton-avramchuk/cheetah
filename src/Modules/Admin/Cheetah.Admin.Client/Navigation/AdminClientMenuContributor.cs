using Cheetah.Core.DependencyInjection;
using Cheetah.Frontend.Navigation.Constants;
using Cheetah.Frontend.Navigation.Extensions;
using Cheetah.Frontend.Navigation.Services.Abstractions;

namespace Cheetah.Admin.Client.Navigation;

[Export(LifetimeType.Singleton, typeof(IMenuContributor))]
public class AdminClientMenuContributor : IMenuContributor
{
    public Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        var mainMenu = context.GetOrCreateMenu(StandardMenus.Main);

        mainMenu
            .AddItem(
                id: "Home",
                name: "Home",
                icon: "bi bi-house-door-fill-nav-menu",
                url: "",
                order: 0)
            .AddItem(
                id: "Counter",
                name: "Counter",
                icon: "bi bi-plus-square-fill-nav-menu",
                url: "counter",
                order: 1)
            .AddItem(
                id: "Weather",
                name: "Weather",
                icon: "bi bi-list-nested-nav-menu",
                url: "weather",
                order: 2);

        return Task.CompletedTask;
    }
}
