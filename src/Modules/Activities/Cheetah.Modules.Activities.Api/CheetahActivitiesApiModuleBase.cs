using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Extensions;
using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Activities.Api.Endpoints;
using Cheetah.Modules.Activities.Application;
using Cheetah.Modules.Activities.Contracts;

namespace Cheetah.Modules.Activities.Api;

/// <summary>
/// Абстрактный базовый Api-модуль: маппит эндпоинты активности в
/// <see cref="OnApplicationInitialization"/>. Наследник закрывает generic-параметры своими
/// конкретными типами эндпоинтов/Contracts:
/// <code>
/// public sealed class AppActivitiesApiModule
///     : CheetahActivitiesApiModuleBase&lt;AppActivityEndpoints, CreateActivityRequest, UpdateActivityRequest, ActivityDto&gt; { }
/// </code>
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmAspNetCoreModule),
    typeof(CheetahActivitiesApplicationModule),
    typeof(CheetahActivitiesContractsModule))]
public abstract class CheetahActivitiesApiModuleBase<TEndpoints, TCreateRequest, TUpdateRequest, TDto> : CrmModule
    where TEndpoints : ActivityEndpointsBase<TCreateRequest, TUpdateRequest, TDto>, new()
    where TCreateRequest : CreateActivityRequestBase
    where TUpdateRequest : UpdateActivityRequestBase
    where TDto : ActivityDtoBase
{
    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        new TEndpoints().Map(context.GetRouteBuilder());
    }
}
