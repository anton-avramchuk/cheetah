using Cheetah.Core.Modules;
using Cheetah.Mapping.Core;
using Mapster;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Mapping.Mapster;

[DependsOn(
    typeof(CrmMappingCoreModule)
    )]
public class CrmMapsterModule:CrmModule
{
    public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(TypeAdapterConfig.GlobalSettings);
        services.AddScoped(AddMapper);
    }
    
    private IObjectMapper AddMapper(IServiceProvider provider)
    {
        var config = provider.GetRequiredService<TypeAdapterConfig>();

        provider.GetServices<IMapsterMappingProfile>()
            .ToList()
            .ForEach(profile => profile.Configure(config));


        return new MapsterObjectMapper(config);
    }
}