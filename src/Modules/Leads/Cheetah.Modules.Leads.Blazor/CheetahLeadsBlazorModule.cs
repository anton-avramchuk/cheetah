using Cheetah.AspNetCore.Blazor.Grid;
using Cheetah.Core;
using Cheetah.Core.Modularity;

namespace Cheetah.Modules.Leads.Blazor;

/// <summary>
/// Blazor Server BFF UI-слой модуля Leads: generic-шаблон грида лидов (закрывается приложением, т.к.
/// модуль абстрактен по <c>LeadBase</c>). Меню формирует приложение, не модуль.
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmBlazorGridModule))]
public partial class CheetahLeadsBlazorModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
