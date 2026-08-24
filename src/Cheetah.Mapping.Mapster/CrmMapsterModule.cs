using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.Mapping.Core;
using Cheetah.Mapping.Mapster.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Mapping.Mapster;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmMappingCoreModule))]
public partial class CrmMapsterModule:CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
        context.Services.AddCheetahMapster();
    }
}
