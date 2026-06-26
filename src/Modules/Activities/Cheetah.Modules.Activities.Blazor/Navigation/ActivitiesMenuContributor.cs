using Cheetah.AspNetCore.Blazor.Navigation;
using Cheetah.AspNetCore.Blazor.Navigation.Builders;
using Cheetah.Core.DependencyInjection;

namespace Cheetah.Modules.Activities.Blazor.Navigation;

/// <summary>Пункты бокового меню модуля Activities: секция «Активности».</summary>
[Export(LifetimeType.Singleton, typeof(IMenuContributor))]
public sealed class ActivitiesMenuContributor : IMenuContributor
{
    public string TargetMenuId => Constants.MainMenuId;

    public Task ConfigureMenuAsync(MenuBuilder builder)
    {
        var section = builder.AddSection("activities", "Активности", order: 35);

        section.AddItem("activities-list", "Задачи")
            .WithIcon("bi bi-check2-square")
            .WithUrl("activities")
            .WithOrder(0);

        return Task.CompletedTask;
    }
}
