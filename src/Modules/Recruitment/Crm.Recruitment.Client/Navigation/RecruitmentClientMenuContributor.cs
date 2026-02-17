using Cheetah.Core.DependencyInjection;
using Cheetah.Frontend.Navigation.Constants;
using Cheetah.Frontend.Navigation.Extensions;
using Cheetah.Frontend.Navigation.Services.Abstractions;

namespace Crm.Recruitment.Client.Navigation;

[Export(LifetimeType.Singleton, typeof(IMenuContributor))]
public class RecruitmentClientMenuContributor : IMenuContributor
{
    public Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        var mainMenu = context.GetOrCreateMenu(StandardMenus.Main);

        mainMenu.AddItem(
            id: "Home",
            name: "Home",
            iconName: MenuIcons.HomeFill,
            url: "/",
            order: 0);

        return Task.CompletedTask;
    }
}
