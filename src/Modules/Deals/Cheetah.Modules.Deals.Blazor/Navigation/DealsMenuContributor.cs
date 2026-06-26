using Cheetah.AspNetCore.Blazor.Navigation;
using Cheetah.AspNetCore.Blazor.Navigation.Builders;
using Cheetah.Core.DependencyInjection;

namespace Cheetah.Modules.Deals.Blazor.Navigation;

/// <summary>Пункты бокового меню модуля Deals: секция «Продажи» (сделки и воронки).</summary>
[Export(LifetimeType.Singleton, typeof(IMenuContributor))]
public sealed class DealsMenuContributor : IMenuContributor
{
    public string TargetMenuId => Constants.MainMenuId;

    public Task ConfigureMenuAsync(MenuBuilder builder)
    {
        var section = builder.AddSection("deals", "Продажи", order: 25);

        section.AddItem("deals-list", "Сделки")
            .WithIcon("bi bi-currency-dollar")
            .WithUrl("deals")
            .WithOrder(0);

        section.AddItem("deals-pipelines", "Воронки")
            .WithIcon("bi bi-funnel-fill")
            .WithUrl("pipelines")
            .WithOrder(1);

        return Task.CompletedTask;
    }
}
