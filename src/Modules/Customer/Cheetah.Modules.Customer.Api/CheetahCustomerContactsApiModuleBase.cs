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
/// Абстрактный базовый Api-модуль контактных лиц: маппит вложенные CRUD-эндпоинты
/// (<c>api/customers/{customerId}/contacts</c>) в <see cref="OnApplicationInitialization"/>.
/// Наследник закрывает generic-параметры своими конкретными типами:
/// <code>
/// public sealed class AppCustomerContactsApiModule
///     : CheetahCustomerContactsApiModuleBase&lt;AppContactEndpoints, CreateContactRequest, UpdateContactRequest, ContactDto&gt; { }
/// </code>
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmAspNetCoreModule),
    typeof(CheetahCustomerApplicationModule),
    typeof(CheetahCustomerContractsModule))]
public abstract class CheetahCustomerContactsApiModuleBase<TEndpoints, TCreateRequest, TUpdateRequest, TDto> : CrmModule
    where TEndpoints : ContactEndpointsBase<TCreateRequest, TUpdateRequest, TDto>, new()
    where TCreateRequest : CreateContactRequestBase
    where TUpdateRequest : UpdateContactRequestBase
    where TDto : ContactDtoBase
{
    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        new TEndpoints().Map(context.GetRouteBuilder());
    }
}
