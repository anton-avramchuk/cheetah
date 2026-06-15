using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Extensions;
using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Leads.Api.Endpoints;
using Cheetah.Modules.Leads.Application;
using Cheetah.Modules.Leads.Contracts;

namespace Cheetah.Modules.Leads.Api;

/// <summary>
/// Абстрактный базовый Api-модуль: маппит эндпоинты лида в <see cref="OnApplicationInitialization"/>.
/// Наследник закрывает generic-параметры своими конкретными типами эндпоинтов/Contracts:
/// <code>
/// public sealed class AppLeadsApiModule
///     : CheetahLeadsApiModuleBase&lt;AppLeadEndpoints, CreateLeadRequest, UpdateLeadRequest, ConvertLeadRequest, LeadDto&gt; { }
/// </code>
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmAspNetCoreModule),
    typeof(CheetahLeadsApplicationModule),
    typeof(CheetahLeadsContractsModule))]
public abstract class CheetahLeadsApiModuleBase<TEndpoints, TCreateRequest, TUpdateRequest, TConvertRequest, TDto> : CrmModule
    where TEndpoints : LeadEndpointsBase<TCreateRequest, TUpdateRequest, TConvertRequest, TDto>, new()
    where TCreateRequest : CreateLeadRequestBase
    where TUpdateRequest : UpdateLeadRequestBase
    where TConvertRequest : ConvertLeadRequestBase
    where TDto : LeadDtoBase
{
    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        new TEndpoints().Map(context.GetRouteBuilder());
    }
}
