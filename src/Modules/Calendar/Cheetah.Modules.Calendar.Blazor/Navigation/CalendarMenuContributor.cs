using Cheetah.AspNetCore.Blazor.Navigation;
using Cheetah.AspNetCore.Blazor.Navigation.Builders;
using Cheetah.Core.DependencyInjection;

namespace Cheetah.Modules.Calendar.Blazor.Navigation;

/// <summary>Пункты бокового меню модуля Calendar: секция «Календарь».</summary>
[Export(LifetimeType.Singleton, typeof(IMenuContributor))]
public sealed class CalendarMenuContributor : IMenuContributor
{
    public string TargetMenuId => Constants.MainMenuId;

    public Task ConfigureMenuAsync(MenuBuilder builder)
    {
        var section = builder.AddSection("calendar", "Календарь", order: 40);

        section.AddItem("calendar-view", "Календарь")
            .WithIcon("bi bi-calendar3")
            .WithUrl("calendar")
            .WithOrder(0);

        return Task.CompletedTask;
    }
}
