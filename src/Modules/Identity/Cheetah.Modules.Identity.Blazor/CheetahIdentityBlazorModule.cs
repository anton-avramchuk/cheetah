using Cheetah.AspNetCore.Blazor.Grid;
using Cheetah.Core;
using Cheetah.Core.Modularity;

namespace Cheetah.Modules.Identity.Blazor;

/// <summary>
/// Blazor Server BFF UI-слой модуля Identity: generic-шаблоны грида пользователей и ролей (закрываются
/// конкретным приложением — Identity абстрактен по <c>IdentityUser</c>/<c>IdentityRole</c>). Меню
/// формирует приложение, не модуль.
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmBlazorGridModule))]
public partial class CheetahIdentityBlazorModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
