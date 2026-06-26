using Cheetah.AspNetCore.Blazor.Controls;
using Cheetah.AspNetCore.Blazor.Grid;
using Cheetah.AspNetCore.Blazor.Layouts;
using Cheetah.AspNetCore.Blazor.Navigation;
using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Catalog.Domain;

namespace Cheetah.Modules.Catalog.Blazor;

/// <summary>
/// Blazor Server BFF UI-слой модуля Catalog: страницы категорий и прайс-листов (грид + CRUD-диалоги),
/// generic-шаблон страницы товаров и пункты бокового меню. CRUD-сервисы и contributor меню регистрируются
/// генератором по <c>[Export]</c>. Хост подключает сборку через <c>AddAdditionalAssemblies(...)</c>.
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmBlazorGridModule))]
[DependsOn(typeof(CrmBlazorNavigationModule))]
[DependsOn(typeof(CrmBlazorLayoutsModule))]
[DependsOn(typeof(CrmBlazorControlsModule))]
[DependsOn(typeof(CheetahCatalogDomainModule))]
public partial class CheetahCatalogBlazorModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
