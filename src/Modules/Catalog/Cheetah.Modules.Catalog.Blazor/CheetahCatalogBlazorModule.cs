using Cheetah.AspNetCore.Blazor.Controls;
using Cheetah.AspNetCore.Blazor.Grid;
using Cheetah.AspNetCore.Blazor.Layouts;
using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Catalog.Domain;

namespace Cheetah.Modules.Catalog.Blazor;

/// <summary>
/// Blazor Server BFF UI-слой модуля Catalog: страницы категорий и прайс-листов (грид + CRUD-диалоги) и
/// generic-шаблон страницы товаров. CRUD-сервисы регистрируются генератором по <c>[Export]</c>. Хост
/// подключает сборку через <c>AddAdditionalAssemblies(...)</c>. Меню формирует приложение, не модуль.
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmBlazorGridModule))]
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
