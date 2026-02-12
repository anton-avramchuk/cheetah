using Cheetah.Blazor.Components.Icons;
using Cheetah.Core.DependencyInjection;
using Cheetah.Frontend.Navigation.Constants;
using Cheetah.Frontend.Navigation.Extensions;
using Cheetah.Frontend.Navigation.Services.Abstractions;

namespace Crm.Candidates.Frontend.Navigation;

[Export(LifetimeType.Singleton, typeof(IMenuContributor))]
public class CandidatesMenuContributor : IMenuContributor
{
    public Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        var mainMenu = context.GetOrCreateMenu(StandardMenus.Main);

        mainMenu.AddItem(
            id: "Candidates",
            name: "Candidates",
            iconName: MenuIcons.People,
            url: "candidates",
            order: 2);

        return Task.CompletedTask;
    }
}
