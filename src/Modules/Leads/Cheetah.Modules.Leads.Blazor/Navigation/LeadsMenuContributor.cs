using Cheetah.AspNetCore.Blazor.Navigation;
using Cheetah.AspNetCore.Blazor.Navigation.Builders;
using Cheetah.Core.DependencyInjection;

namespace Cheetah.Modules.Leads.Blazor.Navigation;

/// <summary>Пункты бокового меню модуля Leads: секция «Лиды».</summary>
[Export(LifetimeType.Singleton, typeof(IMenuContributor))]
public sealed class LeadsMenuContributor : IMenuContributor
{
    public string TargetMenuId => Constants.MainMenuId;

    public Task ConfigureMenuAsync(MenuBuilder builder)
    {
        var section = builder.AddSection("leads", "Лиды", order: 28);

        section.AddItem("leads-list", "Лиды")
            .WithIcon("bi bi-magnet-fill")
            .WithUrl("leads")
            .WithOrder(0);

        return Task.CompletedTask;
    }
}
