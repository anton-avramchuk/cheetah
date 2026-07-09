using Cheetah.AspNetCore.Abstractions;
using Cheetah.Core.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Cheetah.Modules.FeatureManagement.Grpc;

/// <summary>
/// Регистрирует <see cref="FeatureCatalogGrpcService"/> в маршрутизаторе. Вызывается
/// <c>CrmAspNetCoreModule</c> на старте вместе с остальными транспортами (REST не затрагивает).
/// </summary>
[Export(LifetimeType.Singleton, typeof(IModuleTransportRegistrar))]
public sealed class FeatureCatalogGrpcRegistrar : IModuleTransportRegistrar
{
    public void Register(IEndpointRouteBuilder routeBuilder, IServiceProvider serviceProvider)
        => routeBuilder.MapGrpcService<FeatureCatalogGrpcService>();
}
