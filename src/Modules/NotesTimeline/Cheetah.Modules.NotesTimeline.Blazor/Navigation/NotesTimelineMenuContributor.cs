using Cheetah.AspNetCore.Blazor.Navigation;
using Cheetah.AspNetCore.Blazor.Navigation.Builders;
using Cheetah.Core.DependencyInjection;

namespace Cheetah.Modules.NotesTimeline.Blazor.Navigation;

/// <summary>Пункты бокового меню модуля NotesTimeline: секция «Заметки» (заметки и лента хронологии).</summary>
[Export(LifetimeType.Singleton, typeof(IMenuContributor))]
public sealed class NotesTimelineMenuContributor : IMenuContributor
{
    public string TargetMenuId => Constants.MainMenuId;

    public Task ConfigureMenuAsync(MenuBuilder builder)
    {
        var section = builder.AddSection("notes-timeline", "Заметки", order: 55);

        section.AddItem("notes-list", "Заметки")
            .WithIcon("bi bi-sticky-fill")
            .WithUrl("notes")
            .WithOrder(0);

        section.AddItem("timeline-feed", "Лента")
            .WithIcon("bi bi-clock-history")
            .WithUrl("timeline")
            .WithOrder(1);

        return Task.CompletedTask;
    }
}
