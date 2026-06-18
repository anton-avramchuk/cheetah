using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Extensions;
using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.Modularity;
using Cheetah.Modules.Booking.Api.Endpoints;
using Cheetah.Modules.Booking.Application;
using Cheetah.Modules.Booking.Contracts;

namespace Cheetah.Modules.Booking.Api;

/// <summary>
/// Абстрактный базовый Api-модуль Booking: маппит эндпоинты в <see cref="OnApplicationInitialization"/>.
/// Наследник закрывает generic-параметры своими конкретными типами эндпоинтов/Contracts:
/// <code>
/// public sealed class AppBookingApiModule
///     : CheetahBookingApiModuleBase&lt;BookingEndpoints, CreateBookingTypeRequest, UpdateBookingTypeRequest, BookingTypeDto, BookingDto&gt; { }
/// </code>
/// </summary>
[DependsOn(typeof(CoreModule),
    typeof(CrmCQRSCoreModule),
    typeof(CrmAspNetCoreModule),
    typeof(CheetahBookingApplicationModule),
    typeof(CheetahBookingContractsModule))]
public abstract class CheetahBookingApiModuleBase<TEndpoints, TCreateTypeRequest, TUpdateTypeRequest, TBookingTypeDto, TBookingDto>
    : CrmModule
    where TEndpoints : BookingEndpointsBase<TCreateTypeRequest, TUpdateTypeRequest, TBookingTypeDto, TBookingDto>, new()
    where TCreateTypeRequest : CreateBookingTypeRequestBase
    where TUpdateTypeRequest : UpdateBookingTypeRequestBase
    where TBookingTypeDto : BookingTypeDtoBase
    where TBookingDto : BookingDtoBase
{
    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        new TEndpoints().Map(context.GetRouteBuilder());
    }
}
