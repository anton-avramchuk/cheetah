using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Contracts;
using Cheetah.Backend.Endpoints;
using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;
using Cheetah.Modules.SalesDocuments.Application;
using Cheetah.Modules.SalesDocuments.Contracts;

namespace Cheetah.Modules.SalesDocuments.Api;

/// <summary>
/// Api-модуль документов. Декларативные эндпоинты (наследники <c>Cheetah.Backend.Endpoints</c>)
/// регистрируются генератором <c>Cheetah.Generators.Endpoints</c>. Эндпоинты операций
/// (строки/жизненный цикл) работают «из коробки»; CRUD+grid документа — абстрактные шаблоны,
/// закрываемые наследником/хостом (документ расширяем).
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmAspNetCoreModule),
    typeof(CrmAspNetCoreContractsModule),
    typeof(CrmBackendEndpointsModule),
    typeof(CheetahSalesDocumentsApplicationModule),
    typeof(CheetahSalesDocumentsContractsModule))]
public partial class CheetahSalesDocumentsApiModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
