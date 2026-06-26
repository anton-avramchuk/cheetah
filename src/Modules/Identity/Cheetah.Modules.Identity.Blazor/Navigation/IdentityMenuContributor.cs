using Cheetah.AspNetCore.Blazor.Navigation;
using Cheetah.AspNetCore.Blazor.Navigation.Builders;
using Cheetah.Core.DependencyInjection;

namespace Cheetah.Modules.Identity.Blazor.Navigation;

/// <summary>Пункты бокового меню модуля Identity: секция «Доступ» (пользователи и роли).</summary>
[Export(LifetimeType.Singleton, typeof(IMenuContributor))]
public sealed class IdentityMenuContributor : IMenuContributor
{
    public string TargetMenuId => Constants.MainMenuId;

    public Task ConfigureMenuAsync(MenuBuilder builder)
    {
        var section = builder.AddSection("identity", "Доступ", order: 10);

        section.AddItem("identity-users", "Пользователи")
            .WithIcon("bi bi-person-fill")
            .WithUrl("identity/users")
            .WithOrder(0);

        section.AddItem("identity-roles", "Роли")
            .WithIcon("bi bi-shield-lock-fill")
            .WithUrl("identity/roles")
            .WithOrder(1);

        return Task.CompletedTask;
    }
}
