using Cheetah.AspNetCore;
using Cheetah.Backend.Grpc.Interceptors;
using Cheetah.Core;
using Cheetah.Core.CQRS;
using Cheetah.Core.Domain;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Backend.Grpc;

/// <summary>
/// Подключает gRPC-транспорт: серверную инфраструктуру Grpc.AspNetCore и общий интерсептор ошибок.
/// Сами gRPC-сервисы и их регистрация в маршрутизаторе создаются gRPC-генератором
/// (через IModuleTransportRegistrar).
/// </summary>
/// <remarks>
/// Транспорт НЕ привязан к конкретному мапперу: сгенерированные сервисы используют абстракцию
/// <c>IObjectMapper</c>, реализацию которой (Mapster или самописный <c>CrmCustomMappingModule</c>)
/// выбирает само приложение. Mapster-специфичный профиль proto-конверсий вынесен в opt-in
/// модуль <c>CrmBackendGrpcMapsterModule</c>.
/// </remarks>
[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmCQRSCoreModule))]
[DependsOn(typeof(CrmDomainModule))]
[DependsOn(typeof(CrmAspNetCoreModule))]
public partial class CrmBackendGrpcModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);

        context.Services.AddScoped<CrmExceptionInterceptor>();
        context.Services.AddGrpc(options =>
        {
            options.Interceptors.Add<CrmExceptionInterceptor>();
        });
    }
}
