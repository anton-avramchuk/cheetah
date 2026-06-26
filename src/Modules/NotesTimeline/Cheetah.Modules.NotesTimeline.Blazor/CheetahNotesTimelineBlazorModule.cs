using Cheetah.AspNetCore.Blazor.Navigation;
using Cheetah.Core;
using Cheetah.Core.Modularity;

namespace Cheetah.Modules.NotesTimeline.Blazor;

/// <summary>
/// Blazor Server BFF UI-слой модуля NotesTimeline: пункты бокового меню. Лента хронологии и заметки —
/// бесповоротно нестандартный UI (read-model лента, write-model заметки) — поставляются приложением;
/// сборка закрепляет присутствие модуля в навигации. Contributor меню регистрируется генератором
/// по <c>[Export]</c>.
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmBlazorNavigationModule))]
public partial class CheetahNotesTimelineBlazorModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
