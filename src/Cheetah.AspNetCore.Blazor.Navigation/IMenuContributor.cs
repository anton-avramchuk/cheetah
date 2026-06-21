using Cheetah.AspNetCore.Blazor.Navigation.Builders;

namespace Cheetah.AspNetCore.Blazor.Navigation;

public interface IMenuContributor
{
    string TargetMenuId { get; }
    Task ConfigureMenuAsync(MenuBuilder builder);
}
