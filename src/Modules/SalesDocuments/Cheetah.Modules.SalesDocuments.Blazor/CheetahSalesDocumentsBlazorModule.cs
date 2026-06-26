using Cheetah.AspNetCore.Blazor.Grid;
using Cheetah.Core;
using Cheetah.Core.Modularity;

namespace Cheetah.Modules.SalesDocuments.Blazor;

/// <summary>
/// Blazor Server BFF UI-слой модуля SalesDocuments: generic-шаблон грида документов (КП/заказы/счета —
/// закрывается приложением, т.к. модуль абстрактен по <c>SalesDocumentBase</c>). Меню формирует
/// приложение, не модуль.
/// </summary>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmBlazorGridModule))]
public partial class CheetahSalesDocumentsBlazorModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
