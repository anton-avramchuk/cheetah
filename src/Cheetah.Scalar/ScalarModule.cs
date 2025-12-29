using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Extensions;
using Cheetah.Core;
using Cheetah.Core.Modularity;
using Cheetah.OpenApi;
using Microsoft.AspNetCore.Builder;
using Scalar.AspNetCore;

namespace Cheetah.Scalar;

[DependsOn(typeof(CrmAspNetCoreModule),typeof(OpenApiModule))]
public partial class ScalarModule: CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
    }
    
    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var routeBuilder = context.GetRouteBuilder();

        var options = context.GetOptions<ScalarModuleOptions>();

        routeBuilder.MapScalarApiReference(w =>
        {
            if (!string.IsNullOrWhiteSpace(options.OpenApiPath))
            {
                w.OpenApiRoutePattern = options.OpenApiPath;
            }
        });
    }
}