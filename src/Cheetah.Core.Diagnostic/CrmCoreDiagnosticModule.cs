using System.Reflection;
using Cheetah.Core;
using Microsoft.Extensions.Configuration;
using Cheetah.Core.Diagnostic.Attributes;
using Cheetah.Core.Diagnostic.Interceptors;
using Cheetah.Core.Diagnostic.Options;
using Cheetah.Core.Diagnostic.Proxy;
using Cheetah.Core.Extensions.DependencyInjection;
using Cheetah.Core.Modularity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.Core.Diagnostic;

[DependsOn(typeof(CoreModule))]
public partial class CrmCoreDiagnosticModule : CrmModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        RegisterServices(context.Services);

        var configuration = context.Services.GetConfiguration();
        Configure<DiagnosticsOptions>(configuration.GetSection(DiagnosticsOptions.SectionName));

        context.Services.OnRegistered(ctx =>
        {
            if (HasMeasureTimeAttribute(ctx.ImplementationType))
                ctx.Interceptors.Add(typeof(TimingInterceptor));
        });
    }

    public override void PostConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        var enabled = configuration
            .GetSection(DiagnosticsOptions.SectionName)
            .GetValue(nameof(DiagnosticsOptions.Enabled), defaultValue: true);

        if (!enabled) return;

        var services = context.Services;

        var targets = services
            .Where(d => d.ImplementationType is not null
                     && d.ServiceType.IsInterface
                     && HasMeasureTimeAttribute(d.ImplementationType))
            .ToList();

        foreach (var descriptor in targets)
        {
            var idx = services.IndexOf(descriptor);
            if (idx >= 0)
                services[idx] = CreateProxiedDescriptor(descriptor);
        }
    }

    private static bool HasMeasureTimeAttribute(Type type)
        => type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
               .Any(m => m.GetCustomAttribute<MeasureTimeAttribute>() is not null);

    private static ServiceDescriptor CreateProxiedDescriptor(ServiceDescriptor original)
    {
        var implType = original.ImplementationType!;
        var serviceType = original.ServiceType;

        return ServiceDescriptor.Describe(
            serviceType,
            sp =>
            {
                var target = ActivatorUtilities.CreateInstance(sp, implType);
                var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger(implType);
                var options = sp.GetRequiredService<IOptions<DiagnosticsOptions>>().Value;
                return TimingDispatchProxy.Create(serviceType, target, logger, options);
            },
            original.Lifetime);
    }
}
