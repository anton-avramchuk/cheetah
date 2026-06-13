using Cheetah.AspNetCore.Abstractions;
using Cheetah.AspNetCore.Extensions;
using Cheetah.AspNetCore.Middleware;
using Cheetah.Core;
using Cheetah.Core.Domain;
using Cheetah.Core.Extensions.DependencyInjection;
using Cheetah.Core.Modularity;
using Cheetah.Core.Security;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.AspNetCore;

[DependsOn(typeof(CoreModule))]
[DependsOn(typeof(CrmCoreSecurityModule))]
[DependsOn(typeof(CrmDomainModule))]
public partial class CrmAspNetCoreModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
        context.Services.AddAuthorization();
        context.Services.AddHttpContextAccessor();
        context.Services.TryAddObjectAccessor<IApplicationBuilder>();
        context.Services.TryAddObjectAccessor<IEndpointRouteBuilder>();
        context.Services.AddExceptionHandler<ValidationExceptionHandler>();
        context.Services.AddProblemDetails();
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        app.UseExceptionHandler();
        app.UseRouting();

        // Транспорты (gRPC и др.), зарегистрированные модулями через IModuleTransportRegistrar.
        // REST-эндпоинты по-прежнему регистрируются сгенерированным override'ом в каждом модуле.
        var routeBuilder = context.GetRouteBuilder();
        foreach (var registrar in context.ServiceProvider.GetServices<IModuleTransportRegistrar>())
        {
            registrar.Register(routeBuilder, context.ServiceProvider);
        }
    }
}