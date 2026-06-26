using Cheetah.AspNetCore.Blazor.Grid;
using Cheetah.AspNetCore.Blazor.Navigation;
using Cheetah.Core;
using Cheetah.Core.Modularity;

namespace Cheetah.Modules.Activities.Blazor;

/// <summary>
/// Blazor Server BFF UI-слой модуля Activities: generic-шаблон грида задач/активностей (закрывается
/// приложением, т.к. модуль абстрактен по <c>ActivityBase</c>) и пункты меню. Contributor меню
/// регистрируется генератором по <c>[Export]</c>.
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmBlazorGridModule))]
[DependsOn(typeof(CrmBlazorNavigationModule))]
public partial class CheetahActivitiesBlazorModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
