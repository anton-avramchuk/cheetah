using Cheetah.AspNetCore.Blazor.Grid;
using Cheetah.Core;
using Cheetah.Core.Modularity;

namespace Cheetah.Modules.Deals.Blazor;

/// <summary>
/// Blazor Server BFF UI-слой модуля Deals: generic-шаблон грида (сделки/воронки — закрывается приложением).
/// Меню формирует приложение, не модуль.
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmBlazorGridModule))]
public partial class CheetahDealsBlazorModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
