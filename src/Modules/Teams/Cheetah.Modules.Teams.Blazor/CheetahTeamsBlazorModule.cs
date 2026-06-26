using Cheetah.AspNetCore.Blazor.Controls;
using Cheetah.AspNetCore.Blazor.Grid;
using Cheetah.AspNetCore.Blazor.Layouts;
using Cheetah.AspNetCore.Blazor.Navigation;
using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Teams.Domain;

namespace Cheetah.Modules.Teams.Blazor;

/// <summary>
/// Blazor Server BFF UI-слой модуля Teams: страница ролей (грид + CRUD), generic-шаблон команд и пункты
/// меню. CRUD-сервисы и contributor меню регистрируются генератором по <c>[Export]</c>. Хост подключает
/// сборку через <c>AddAdditionalAssemblies(...)</c>.
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmBlazorGridModule))]
[DependsOn(typeof(CrmBlazorNavigationModule))]
[DependsOn(typeof(CrmBlazorLayoutsModule))]
[DependsOn(typeof(CrmBlazorControlsModule))]
[DependsOn(typeof(CheetahTeamsDomainModule))]
public partial class CheetahTeamsBlazorModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
