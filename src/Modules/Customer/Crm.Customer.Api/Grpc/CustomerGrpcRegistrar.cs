using Cheetah.AspNetCore.Abstractions;
using Cheetah.Core.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Crm.Customer.Api.Grpc;

/// <summary>
/// Регистрирует gRPC-сервисы модуля Customer в общем маршрутизаторе.
/// Вызывается из <c>CrmAspNetCoreModule</c> на старте приложения.
/// В дальнейшем класс будет генерироваться gRPC-генератором.
/// </summary>
[Export(LifetimeType.Singleton, typeof(IModuleTransportRegistrar))]
public sealed class CustomerGrpcRegistrar : IModuleTransportRegistrar
{
    public void Register(IEndpointRouteBuilder routeBuilder, IServiceProvider serviceProvider)
    {
        routeBuilder.MapGrpcService<CustomerGrpcService>();
    }
}
