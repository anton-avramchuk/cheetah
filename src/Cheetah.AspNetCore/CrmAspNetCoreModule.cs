using Cheetah.AspNetCore.Extensions;
using Cheetah.AspNetCore.Middleware;
using Cheetah.Core;
using Cheetah.Core.Domain;
using Cheetah.Core.Extensions.DependencyInjection;
using Cheetah.Core.Modularity;
using Cheetah.Core.Security;

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
        context.Services.AddObjectAccessor<IApplicationBuilder>();
        context.Services.AddObjectAccessor<IEndpointRouteBuilder>();
        context.Services.AddExceptionHandler<ValidationExceptionHandler>();
        context.Services.AddProblemDetails();
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        app.UseExceptionHandler();
        app.UseRouting();

    }
}