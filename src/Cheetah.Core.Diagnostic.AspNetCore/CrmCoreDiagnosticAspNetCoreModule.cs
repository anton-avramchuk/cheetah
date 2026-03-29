using Cheetah.AspNetCore;
using Cheetah.AspNetCore.Extensions;
using Cheetah.Core.Diagnostic.AspNetCore.Filters;
using Cheetah.Core.Diagnostic.Options;
using Cheetah.Core.DependencyInjection;
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
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var options = context.GetOptions<DiagnosticsOptions>();
        if (!options.Enabled) return;

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
