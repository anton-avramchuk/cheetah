using Cheetah.Blazor.Components.Icons;
using Cheetah.Core.DependencyInjection;
using Cheetah.Frontend.Navigation.Constants;
using Cheetah.Frontend.Navigation.Extensions;
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

        return Task.CompletedTask;
    }
}
