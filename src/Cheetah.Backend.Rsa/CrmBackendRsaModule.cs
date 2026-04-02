using Cheetah.Core;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Backend.Rsa;

[DependsOn(typeof(CoreModule))]
public partial class CrmBackendRsaModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);

        context.Services
            .AddOptions<RsaOptions>()
            .BindConfiguration(RsaOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }
}
