using System.Diagnostics;
using System.Reflection;
using Cheetah.Core.Common;
using Cheetah.Core.Diagnostic.Attributes;
using Cheetah.Core.Diagnostic.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.Core.Diagnostic.Interceptors;

public class TimingInterceptor : CrmInterceptor
{
    private readonly ILogger<TimingInterceptor> _logger;
    private readonly DiagnosticsOptions _options;

    public TimingInterceptor(ILogger<TimingInterceptor> logger, IOptions<DiagnosticsOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public override async Task InterceptAsync(ICrmMethodInvocation invocation)
    {
        var attr = invocation.Method.GetCustomAttribute<MeasureTimeAttribute>();

        if (!_options.Enabled || attr is null)
        {
            await invocation.ProceedAsync();
            return;
        }

        var typeName = invocation.TargetObject.GetType().Name;
        var methodName = invocation.Method.Name;
        var label = attr.Label is not null ? $" ({attr.Label})" : string.Empty;

        var sw = Stopwatch.StartNew();
        try
        {
            await invocation.ProceedAsync();
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogWarning(ex,
                "[Diagnostic]{Label} {Type}.{Method} threw after {Elapsed}ms",
                label, typeName, methodName, sw.ElapsedMilliseconds);
            throw;
        }

        sw.Stop();
        _logger.LogInformation(
            "[Diagnostic]{Label} {Type}.{Method} completed in {Elapsed}ms",
            label, typeName, methodName, sw.ElapsedMilliseconds);
    }
}
