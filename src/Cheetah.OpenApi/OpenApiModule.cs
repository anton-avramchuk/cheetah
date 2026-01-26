using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Extensions;
using Cheetah.Core;
using Cheetah.Core.Extensions.DependencyInjection;
using Cheetah.Core.Modularity;

namespace Cheetah.OpenApi;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmAspNetCoreModule))]
public partial class OpenApiModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);

        var configuration = context.Services.GetConfiguration();
        var basePath = configuration["OpenApi:BasePath"] ?? "";

        context.Services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, ctx, ct) =>
            {
                if (!string.IsNullOrEmpty(basePath))
                {
                    document.Servers.Clear();
                    document.Servers.Add(new() { Url = basePath });
                }
                return Task.CompletedTask;
            });
        });
    }


    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var routeBuilder = context.GetRouteBuilder();
        routeBuilder.MapOpenApi();
    }

}