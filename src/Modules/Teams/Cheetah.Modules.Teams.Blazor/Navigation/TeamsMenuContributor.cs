using Cheetah.AspNetCore.Blazor.Navigation;
using Cheetah.AspNetCore.Blazor.Navigation.Builders;
using Cheetah.Core.DependencyInjection;

namespace Cheetah.Modules.Teams.Blazor.Navigation;

/// <summary>Пункты бокового меню модуля Teams: секция «Команды» (команды и роли).</summary>
[Export(LifetimeType.Singleton, typeof(IMenuContributor))]
public sealed class TeamsMenuContributor : IMenuContributor
{
    public string TargetMenuId => Constants.MainMenuId;

    public Task ConfigureMenuAsync(MenuBuilder builder)
    {
        var section = builder.AddSection("teams", "Команды", order: 20);

        section.AddItem("teams-list", "Команды")
            .WithIcon("bi bi-people-fill")
            .WithUrl("teams")
            .WithOrder(0);

        section.AddItem("teams-roles", "Роли команд")
            .WithIcon("bi bi-person-badge")
            .WithUrl("teams/roles")
            .WithOrder(1);

        return Task.CompletedTask;
    }
}
