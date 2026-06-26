using Cheetah.AspNetCore.Blazor.Controls;
using Cheetah.AspNetCore.Blazor.Grid;
using Cheetah.AspNetCore.Blazor.Layouts;
using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Teams.Domain;

namespace Cheetah.Modules.Teams.Blazor;

/// <summary>
/// Blazor Server BFF UI-слой модуля Teams: страница ролей (грид + CRUD) и generic-шаблон команд.
/// CRUD-сервисы регистрируются генератором по <c>[Export]</c>. Хост подключает сборку через
/// <c>AddAdditionalAssemblies(...)</c>. Меню формирует приложение, не модуль.
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmBlazorGridModule))]
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
