using Cheetah.Blazor.Components.Icons;
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
                iconName: MenuIcons.HomeFill,
                url: "",
                order: 0)
            .AddItem(
                id: "Counter",
                name: "Counter",
                iconName: MenuIcons.PlusSquare,
                url: "counter",
                order: 1)
            .AddItem(
                id: "Weather",
                name: "Weather",
                iconName: MenuIcons.Cloud,
                url: "weather",
                order: 2);

        return Task.CompletedTask;
    }
}
