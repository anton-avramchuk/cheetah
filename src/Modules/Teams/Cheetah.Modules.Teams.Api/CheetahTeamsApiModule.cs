using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Contracts;
using Cheetah.Backend.Endpoints;
using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Teams.Application;
using Cheetah.Modules.Teams.Contracts;

namespace Cheetah.Modules.Teams.Api;

/// <summary>
/// Api-модуль команд. Декларативные эндпоинты (наследники <c>Cheetah.Backend.Endpoints</c>)
/// регистрируются генератором <c>Cheetah.Generators.Endpoints</c> в <c>OnApplicationInitialization</c>.
/// Конкретные эндпоинты ролей и участников работают «из коробки»; эндпоинты команд — абстрактные
/// шаблоны, закрываемые наследником/хостом (команда расширяема).
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmAspNetCoreModule),
    typeof(CrmAspNetCoreContractsModule),
    typeof(CrmBackendEndpointsModule),
    typeof(CheetahTeamsApplicationModule),
    typeof(CheetahTeamsContractsModule))]
public partial class CheetahTeamsApiModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
}
