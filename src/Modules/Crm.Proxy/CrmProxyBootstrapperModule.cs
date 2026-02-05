using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Extensions;
using Cheetah.Backend.ReverseProxy;
using Cheetah.Backend.ReverseProxy.Extensions;
using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.OpenApi;
using Cheetah.OpenApi.Extensions;
using Cheetah.Scalar;

namespace Crm.Proxy;

[Bootstrapper]
[DependsOn(typeof(CoreModule),
    typeof(CrmAspNetCoreModule),
    typeof(ScalarModule),
    typeof(CrmBackendReverseProxyModule))]
public partial class CrmProxyBootstrapperModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
        context.Services.AddHttpClient();
        context.Services.AddYarpFromConfig();
        context.Services.AddOpenApiAggregator();
        
        Configure<ScalarModuleOptions>(w =>
        {
            w.OpenApiPath = OpenApiConstants.OpenApiCombinedDocumentPath;
        });
    }


    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var builder = context.GetRouteBuilder();
        builder.UseCombinedOpenApi();
    }
}