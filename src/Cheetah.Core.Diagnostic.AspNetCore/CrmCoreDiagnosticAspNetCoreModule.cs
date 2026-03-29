using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Extensions;
using Cheetah.Core.Diagnostic.AspNetCore.Filters;
using Cheetah.Core.Diagnostic.AspNetCore.Middleware;
using Cheetah.Core.Diagnostic.AspNetCore.Options;
using Cheetah.Core.Diagnostic.Options;
using Cheetah.Core.DependencyInjection;
using Cheetah.Core.Extensions.DependencyInjection;
using Cheetah.Core.Modularity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Core.Diagnostic.AspNetCore;

[DependsOn(typeof(CrmCoreDiagnosticModule))]
[DependsOn(typeof(CrmAspNetCoreModule))]
public partial class CrmCoreDiagnosticAspNetCoreModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);
        context.Services.AddScoped<TimingEndpointFilter>();

        var configuration = context.Services.GetConfiguration();
        Configure<RequestLoggingOptions>(
            configuration.GetSection(
                $"{DiagnosticsOptions.SectionName}:{RequestLoggingOptions.SubSection}"));
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var options = context.GetOptions<DiagnosticsOptions>();
        if (!options.Enabled) return;

        var app = context.GetApplicationBuilder();

        // Request/response logging — registered before endpoint execution so it captures
        // both the incoming request and the final response status code.
        var requestLoggingOptions = context.GetOptions<RequestLoggingOptions>();
        if (requestLoggingOptions.Enabled)
            app.UseMiddleware<RequestLoggingMiddleware>();

        // Replace the shared IEndpointRouteBuilder with a group that has TimingEndpointFilter.
        // All subsequent modules resolve GetRouteBuilder() and register their routes on this
        // group — the filter is inherited automatically by every endpoint in it.
        var accessor = context.ServiceProvider
            .GetRequiredService<ObjectAccessor<IEndpointRouteBuilder>>();

        if (accessor.Value is null) return;

        var group = accessor.Value.MapGroup(string.Empty);
        group.AddEndpointFilter<TimingEndpointFilter>();

        accessor.Value = group;
    }
}
