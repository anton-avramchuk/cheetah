using Cheetah.AspNetCore;
using Cheetah.Core.Modules;

namespace Cheetah.OpenApi;

[DependsOn(typeof(CrmAspNetCoreModule))]
public class OpenApiModule : CrmModule
{
    public override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenApi();
    }
}