using Cheetah.Blazor.Components.Icons;
using Cheetah.Core.DependencyInjection;
using Cheetah.Frontend.Navigation.Constants;
using Cheetah.Frontend.Navigation.Extensions;
using Cheetah.Frontend.Navigation.Models;
using Cheetah.Frontend.Navigation.Services.Abstractions;

namespace Cheetah.Admin.Modules.Clients.Frontend.Navigation;

[Export(LifetimeType.Singleton, typeof(IMenuContributor))]
public class ClientsMenuContributor : IMenuContributor
{
    public Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        var mainMenu = context.GetOrCreateMenu(StandardMenus.Main);

        mainMenu.AddItem(
            id: "Clients",
            name: "Clients",
            iconName: MenuIcons.PeopleFill,
            url: "clients",
            order: 3);

        var dictionary = mainMenu.GetOrAdd("Dictionary", item =>
        {
            item.Name = "Dictionary";
            item.IconName = MenuIcons.ListNested;
            item.Order = 10;
        });

        dictionary.AddChild(
            id: "Tariffs",
            name: "Tariffs",
            url: "dictionary/tariffs",
            order: 1);

        return Task.CompletedTask;
    }
}
