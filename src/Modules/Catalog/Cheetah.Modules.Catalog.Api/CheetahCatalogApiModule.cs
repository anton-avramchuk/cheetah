using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Contracts;
using Cheetah.Backend.Endpoints;
using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Catalog.Application;
using Cheetah.Modules.Catalog.Contracts;

namespace Cheetah.Modules.Catalog.Api;

/// <summary>
/// Api-модуль каталога. Декларативные эндпоинты (наследники <c>Cheetah.Backend.Endpoints</c>)
/// регистрируются генератором <c>Cheetah.Generators.Endpoints</c> в <c>OnApplicationInitialization</c>.
/// Конкретные эндпоинты категорий и прайс-листов работают «из коробки»; эндпоинты товаров —
/// абстрактные шаблоны, закрываемые наследником/хостом (товар расширяем).
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmAspNetCoreModule),
    typeof(CrmAspNetCoreContractsModule),
    typeof(CrmBackendEndpointsModule),
    typeof(CheetahCatalogApplicationModule),
    typeof(CheetahCatalogContractsModule))]
public partial class CheetahCatalogApiModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
