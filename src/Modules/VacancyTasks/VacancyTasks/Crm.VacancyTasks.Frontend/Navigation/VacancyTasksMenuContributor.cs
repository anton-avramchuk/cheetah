using Cheetah.Blazor.Components.Icons;
using Cheetah.Core.DependencyInjection;
using Cheetah.Frontend.Navigation.Constants;
using Cheetah.Frontend.Navigation.Extensions;
using Cheetah.Frontend.Navigation.Models;
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
            url: "vacancy-tasks/board",
            order: 3);

        var dictionary = mainMenu.GetOrAdd("Dictionary", item =>
        {
            item.Name = "Dictionary";
            item.IconName = MenuIcons.ListNested;
            item.Order = 10;
        });

        dictionary
            .AddChild(
                id: "TaskStates",
                name: "Task States",
                url: "dictionary/task-states",
                order: 8)
            .AddChild(
                id: "TaskPriorities",
                name: "Task Priorities",
                url: "dictionary/task-priorities",
                order: 9);

        return Task.CompletedTask;
    }
}
