using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Extensions;
using Cheetah.Core;
using Cheetah.Core.Modularity;

namespace Cheetah.OpenApi;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmAspNetCoreModule))]
public partial class OpenApiModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
        context.Services.AddOpenApi();
    }
    
    
    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var routeBuilder = context.GetRouteBuilder();
        routeBuilder.MapOpenApi();
    }

}