using Cheetah.Blazor.Components.Icons;
using Cheetah.Core.DependencyInjection;
using Cheetah.Frontend.Navigation.Constants;
using Cheetah.Frontend.Navigation.Extensions;
using Cheetah.Frontend.Navigation.Models;
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

        var dictionary = mainMenu.GetOrAdd("Dictionary", item =>
        {
            item.Name = "Dictionary";
            item.IconName = MenuIcons.ListNested;
            item.Order = 10;
        });

        dictionary
            .AddChild(
                id: "Directions",
                name: "Directions",
                url: "dictionary/directions",
                order: 1)
            .AddChild(
                id: "Positions",
                name: "Positions",
                url: "dictionary/positions",
                order: 2)
            .AddChild(
                id: "StackItems",
                name: "Stack",
                url: "dictionary/stack-items",
                order: 3)
            .AddChild(
                id: "WorkFormats",
                name: "Work Formats",
                url: "dictionary/work-formats",
                order: 4)
            .AddChild(
                id: "Customers",
                name: "Customers",
                url: "dictionary/customers",
                order: 5);

        return Task.CompletedTask;
    }
}
