using Cheetah.Blazor.Components.Icons;
using Cheetah.Core.DependencyInjection;
using Cheetah.Frontend.Navigation.Constants;
using Cheetah.Frontend.Navigation.Extensions;
using Cheetah.Frontend.Navigation.Services.Abstractions;

namespace Crm.Recruitment.Frontend.Navigation;

[Export(LifetimeType.Singleton, typeof(IMenuContributor))]
public class RecruitmentMenuContributor : IMenuContributor
{
    public Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        var mainMenu = context.GetOrCreateMenu(StandardMenus.Main);

        mainMenu.AddItem(
            id: "Vacancies",
            name: "Vacancies",
            iconName: MenuIcons.Briefcase,
            url: "vacancies",
            order: 1);

        return Task.CompletedTask;
    }
}
