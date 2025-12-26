using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Mapping.Mapster.DependencyInjection;

public static class RegistrationExtensions
{
    public static IServiceCollection AddMapping<TMapping>(this IServiceCollection services) where TMapping : class, IMapsterMappingProfile
    {
        services.AddScoped<IMapsterMappingProfile, TMapping>();
        return services;
    }
}