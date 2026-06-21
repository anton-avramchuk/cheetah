using Cheetah.AspNetCore.Blazor.Navigation.Models;

namespace Cheetah.AspNetCore.Blazor.Navigation;

public interface INavigationMenuService
{
    Task<Menu> GetMenuAsync(string menuId);
}
