using Cheetah.Backend.ReverseProxy.Services;
using Cheetah.Core.Extensions.DependencyInjection;
using Cheetah.OpenApi.Services;
using Yarp.ReverseProxy.Configuration;

namespace Cheetah.Backend.ReverseProxy.Extensions;

public static class ServiceCollectionsExtensions
{
    public static IServiceCollection AddYarpFromConfig(this IServiceCollection services,
        string configurationKey = Constants.YarpSectionName)
    {
        var configuration = services.GetConfiguration();

        services.AddSingleton<IClusterAddressProvider>(w =>
            new ConfiguredClusterAddressProvider(configuration, configurationKey));

        var yarpConfig = configuration.GetSection(Constants.YarpSectionName);

        services.AddReverseProxy()
            .LoadFromConfig(yarpConfig)
            .AddServiceDiscoveryDestinationResolver();
        return services;
    }


    public static IServiceCollection AddYarpFromMemory(this IServiceCollection services,
        IReadOnlyList<RouteConfig> routes,
        IReadOnlyList<ClusterConfig> clusters)
    {
        services.AddSingleton<IClusterAddressProvider>(w => new InMemoryClusterAddressProvider(routes, clusters));

        services.AddReverseProxy()
            .LoadFromMemory(routes, clusters)
            .AddServiceDiscoveryDestinationResolver();

        return services;
    }
}