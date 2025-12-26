using Cheetah.OpenApi.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.OpenApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOpenApiAggregator(this IServiceCollection services)
    {
        services.AddSingleton<IOpenApiAggregator, OpenApiAggregator>();

        return services;
    }
}