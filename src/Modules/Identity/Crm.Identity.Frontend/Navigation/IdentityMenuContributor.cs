using System.Threading.Tasks;
using Cheetah.Core.DependencyInjection;
using Cheetah.Frontend.Navigation.Constants;
using Cheetah.Frontend.Navigation.Extensions;
using Cheetah.Frontend.Navigation.Models;
using Cheetah.Frontend.Navigation.Services.Abstractions;

namespace Crm.Identity.Frontend.Navigation;

[Export(LifetimeType.Singleton, typeof(IMenuContributor))]
public class IdentityMenuContributor : IMenuContributor
{
    private const string AdminRole = "admin";
    public Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        var mainMenu = context.GetOrCreateMenu(StandardMenus.Main);

        var admin = mainMenu.GetOrAdd("Admin", item =>
        {
            item.Name = "Admin";
            item.IconName = MenuIcons.PersonGear;
            item.Order = 100;
        });

        admin
            .AddChild(
                id: "Users",
                name: "Users",
                url: "admin/users",
                requiredRoles: [AdminRole],
                order: 1)
            .AddChild(
                id: "Roles",
                name: "Roles",
                url: "admin/roles",
                requiredRoles: [AdminRole],
                order: 2);

        return Task.CompletedTask;
    }
}
