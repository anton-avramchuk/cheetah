using Cheetah.AspNetCore.Blazor.Grid;
using Cheetah.AspNetCore.Blazor.Navigation;
using Cheetah.Core;
using Cheetah.Core.Modularity;

namespace Cheetah.Modules.Deals.Blazor;

/// <summary>
/// Blazor Server BFF UI-слой модуля Deals: generic-шаблон грида (сделки/воронки — закрывается приложением)
/// и пункты меню «Продажи». Contributor меню регистрируется генератором по <c>[Export]</c>.
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmBlazorGridModule))]
[DependsOn(typeof(CrmBlazorNavigationModule))]
public partial class CheetahDealsBlazorModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
