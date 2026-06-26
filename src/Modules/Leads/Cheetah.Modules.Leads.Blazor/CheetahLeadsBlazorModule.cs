using Cheetah.AspNetCore.Blazor.Grid;
using Cheetah.AspNetCore.Blazor.Navigation;
using Cheetah.Core;
using Cheetah.Core.Modularity;

namespace Cheetah.Modules.Leads.Blazor;

/// <summary>
/// Blazor Server BFF UI-слой модуля Leads: generic-шаблон грида лидов (закрывается приложением, т.к.
/// модуль абстрактен по <c>LeadBase</c>) и пункты меню. Contributor меню регистрируется генератором
/// по <c>[Export]</c>.
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmBlazorGridModule))]
[DependsOn(typeof(CrmBlazorNavigationModule))]
public partial class CheetahLeadsBlazorModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
