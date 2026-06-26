using Cheetah.AspNetCore.Blazor.Grid;
using Cheetah.AspNetCore.Blazor.Navigation;
using Cheetah.Core;
using Cheetah.Core.Modularity;

namespace Cheetah.Modules.Identity.Blazor;

/// <summary>
/// Blazor Server BFF UI-слой модуля Identity: generic-шаблоны грида пользователей и ролей (закрываются
/// конкретным приложением — Identity абстрактен по <c>IdentityUser</c>/<c>IdentityRole</c>) и пункты меню.
/// Contributor меню регистрируется генератором по <c>[Export]</c>.
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmBlazorGridModule))]
[DependsOn(typeof(CrmBlazorNavigationModule))]
public partial class CheetahIdentityBlazorModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
