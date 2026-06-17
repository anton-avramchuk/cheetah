using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.DataAccess;
using Cheetah.Core.Events;
using Cheetah.Core.Grid;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Catalog.Contracts;
using Cheetah.Modules.Catalog.Domain;
using Cheetah.Modules.Catalog.DomainEvents;

namespace Cheetah.Modules.Catalog.Application;

/// <summary>
/// Прикладной слой шаблонного модуля Catalog: generic CQRS товаров + конкретные хендлеры категорий
/// и прайс-листов (регистрируются генератором по <c>[Export]</c>). Закрытые generic-handler'ы товара
/// регистрирует наследник через <c>AddCatalogApplication&lt;…&gt;()</c>.
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmDataAccessModule),
    typeof(CrmEventsCoreModule),
    typeof(CrmGridModule),
    typeof(CheetahCatalogDomainModule),
    typeof(CheetahCatalogContractsModule),
    typeof(CheetahCatalogDomainEventsModule))]
public partial class CheetahCatalogApplicationModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
