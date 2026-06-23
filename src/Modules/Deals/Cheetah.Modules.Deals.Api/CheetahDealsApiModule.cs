using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Contracts;
using Cheetah.Backend.Endpoints;
using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;
using Cheetah.Mapping.Core;
using Cheetah.Modules.Deals.Application;
using Cheetah.Modules.Deals.Contracts;

namespace Cheetah.Modules.Deals.Api;

/// <summary>
/// HTTP-слой Deals. Эндпоинты описаны декларативно классами в <c>Endpoints/</c> (наследники
/// <c>Cheetah.Backend.Endpoints</c>); их регистрацию генерирует <c>Cheetah.Generators.Endpoints</c>.
/// Реквест→команда/запрос — через Mapping.
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmAspNetCoreModule),
    typeof(CrmAspNetCoreContractsModule),
    typeof(CrmBackendEndpointsModule),
    typeof(CrmMappingCoreModule),
    typeof(CheetahDealsApplicationModule),
    typeof(CheetahDealsContractsModule))]
public partial class CheetahDealsApiModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
