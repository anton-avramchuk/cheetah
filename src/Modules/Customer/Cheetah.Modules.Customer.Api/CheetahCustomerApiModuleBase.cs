using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Extensions;
using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Customer.Api.Endpoints;
using Cheetah.Modules.Customer.Application;
using Cheetah.Modules.Customer.Contracts;

namespace Cheetah.Modules.Customer.Api;

/// <summary>
/// Абстрактный базовый Api-модуль: маппит CRUD-эндпоинты клиента в
/// <see cref="OnApplicationInitialization"/>. Наследник закрывает generic-параметры своими
/// конкретными типами эндпоинтов/Contracts и наследует зависимости через <c>[DependsOn]</c>:
/// <code>
/// public sealed class AppCustomerApiModule
///     : CheetahCustomerApiModuleBase&lt;AppCustomerEndpoints, CreateCustomerRequest, UpdateCustomerRequest, CustomerDto&gt; { }
/// </code>
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmAspNetCoreModule),
    typeof(CheetahCustomerApplicationModule),
    typeof(CheetahCustomerContractsModule))]
public abstract class CheetahCustomerApiModuleBase<TEndpoints, TCreateRequest, TUpdateRequest, TDto> : CrmModule
    where TEndpoints : CustomerEndpointsBase<TCreateRequest, TUpdateRequest, TDto>, new()
    where TCreateRequest : CreateCustomerRequestBase
    where TUpdateRequest : UpdateCustomerRequestBase
    where TDto : CustomerDtoBase
{
    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        new TEndpoints().Map(context.GetRouteBuilder());
    }
}
