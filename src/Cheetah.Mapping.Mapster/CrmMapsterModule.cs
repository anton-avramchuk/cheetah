using Cheetah.Core.Modularity;
using Cheetah.Mapping.Core;
using Mapster;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Mapping.Mapster;

[DependsOn(
    typeof(CrmMappingCoreModule)
    )]
public partial class CrmMapsterModule:CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
        context.Services.AddSingleton(TypeAdapterConfig.GlobalSettings);
        context.Services.AddScoped(AddMapper);
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