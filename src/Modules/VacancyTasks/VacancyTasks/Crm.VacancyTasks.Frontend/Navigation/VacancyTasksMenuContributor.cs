using Cheetah.Blazor.Components.Icons;
using Cheetah.Core.DependencyInjection;
using Cheetah.Frontend.Navigation.Constants;
using Cheetah.Frontend.Navigation.Extensions;
using Cheetah.Frontend.Navigation.Services.Abstractions;

namespace Crm.VacancyTasks.Frontend.Navigation;

[Export(LifetimeType.Singleton, typeof(IMenuContributor))]
public class VacancyTasksMenuContributor : IMenuContributor
{
    public Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        var mainMenu = context.GetOrCreateMenu(StandardMenus.Main);

        mainMenu.AddItem(
            id: "VacancyTasks",
            name: "Tasks",
            iconName: MenuIcons.ListTask,
            url: "vacancy-tasks",
            order: 3);

        return Task.CompletedTask;
    }
}
