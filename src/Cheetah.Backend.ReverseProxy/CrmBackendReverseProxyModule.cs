using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Extensions;
using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.OpenApi;

namespace Cheetah.Backend.ReverseProxy;

[DependsOn(typeof(CoreModule), typeof(CrmAspNetCoreModule), typeof(OpenApiModule))]
public partial class CrmBackendReverseProxyModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var routeBuilder = context.GetRouteBuilder();
        routeBuilder.MapReverseProxy();
    }
}