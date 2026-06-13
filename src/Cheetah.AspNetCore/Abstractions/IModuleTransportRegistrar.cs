using Microsoft.AspNetCore.Routing;

namespace Cheetah.AspNetCore.Abstractions;

/// <summary>
/// Точка расширения для регистрации транспортов (REST, gRPC и др.) на старте приложения.
/// Каждая реализация регистрируется в DI через <c>[Export(LifetimeType.Singleton, typeof(IModuleTransportRegistrar))]</c>
/// и вызывается один раз из <see cref="CrmAspNetCoreModule"/> в <c>OnApplicationInitialization</c>.
/// </summary>
/// <remarks>
/// Механизм намеренно не использует <c>OnApplicationInitialization</c>-override модуля,
/// чтобы несколько генераторов (endpoints, gRPC, ...) не конфликтовали за единственный override.
/// </remarks>
public interface IModuleTransportRegistrar
{
    /// <summary>
    /// Регистрирует эндпоинты транспорта в общем <see cref="IEndpointRouteBuilder"/>.
    /// </summary>
    void Register(IEndpointRouteBuilder routeBuilder, IServiceProvider serviceProvider);
}
