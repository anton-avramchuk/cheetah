using Cheetah.Frontend.Navigation.Constants;
using Cheetah.Frontend.Navigation.Extensions;
using Cheetah.Frontend.Navigation.Services.Abstractions;

namespace Cheetah.Frontend.Auth.Navigation;

public class UserMenuContributor : IMenuContributor
{
    public Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        var userMenu = context.GetOrCreateMenu(StandardMenus.User);

        userMenu.AddItem(
            id: "Logout",
            name: "Logout",
            iconName: "box-arrow-right",
            url: "/logout",
            order: 100);

        return Task.CompletedTask;
    }
}
