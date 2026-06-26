using Cheetah.AspNetCore.Blazor.Grid;
using Cheetah.Core;
using Cheetah.Core.Modularity;

namespace Cheetah.Modules.Activities.Blazor;

/// <summary>
/// Blazor Server BFF UI-слой модуля Activities: generic-шаблон грида задач/активностей (закрывается
/// приложением, т.к. модуль абстрактен по <c>ActivityBase</c>). Меню формирует приложение, не модуль.
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmBlazorGridModule))]
public partial class CheetahActivitiesBlazorModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
