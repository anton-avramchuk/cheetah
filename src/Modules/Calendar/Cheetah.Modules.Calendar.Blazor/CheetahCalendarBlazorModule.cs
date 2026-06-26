using Cheetah.AspNetCore.Blazor.Navigation;
using Cheetah.Core;
using Cheetah.Core.Modularity;

namespace Cheetah.Modules.Calendar.Blazor;

/// <summary>
/// Blazor Server BFF UI-слой модуля Calendar: пункты бокового меню. Само представление календаря —
/// бесповоротно нестандартный UI (вид-календарь, события, напоминания) — поставляется приложением;
/// эта сборка закрепляет присутствие модуля в навигации. Contributor меню регистрируется генератором
/// по <c>[Export]</c>.
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmBlazorNavigationModule))]
public partial class CheetahCalendarBlazorModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
