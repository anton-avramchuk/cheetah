using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Contracts;
using Cheetah.Backend.Endpoints;
using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;
using Cheetah.Mapping.Mapster;
using Cheetah.Modules.Calendar.Application;
using Cheetah.Modules.Calendar.Contracts;

namespace Cheetah.Modules.Calendar.Api;

/// <summary>
/// HTTP-слой Calendar. Эндпоинты описаны декларативно классами в <c>Endpoints/</c> (наследники
/// <c>Cheetah.Backend.Endpoints</c>); их регистрацию в <see cref="CrmModule.OnApplicationInitialization"/>
/// генерирует <c>Cheetah.Generators.Endpoints</c>. Реквест→команда/запрос — через Mapster-профиль.
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmAspNetCoreModule),
    typeof(CrmAspNetCoreContractsModule),
    typeof(CrmMapsterModule),
    typeof(CrmBackendEndpointsModule),
    typeof(CheetahCalendarApplicationModule),
    typeof(CheetahCalendarContractsModule))]
public partial class CheetahCalendarApiModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services); // регистрирует Mapster-профиль (auto-generated)
    }
}
