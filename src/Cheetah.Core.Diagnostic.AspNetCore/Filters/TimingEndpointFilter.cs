using System.Diagnostics;
using Cheetah.Core.Diagnostic.Attributes;
using Cheetah.Core.Diagnostic.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.Core.Diagnostic.AspNetCore.Filters;

public class TimingEndpointFilter : IEndpointFilter
{
    private readonly ILogger<TimingEndpointFilter> _logger;
    private readonly DiagnosticsOptions _options;

    public TimingEndpointFilter(ILogger<TimingEndpointFilter> logger, IOptions<DiagnosticsOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var attr = context.HttpContext.GetEndpoint()?.Metadata.GetMetadata<MeasureTimeAttribute>();

        if (!_options.Enabled || attr is null)
            return await next(context);

        var endpointName = context.HttpContext.GetEndpoint()?.DisplayName
                           ?? context.HttpContext.Request.Path.ToString();
        var label = attr.Label is not null ? $" ({attr.Label})" : string.Empty;

        var sw = Stopwatch.StartNew();
        try
        {
            var result = await next(context);
            sw.Stop();
            _logger.LogInformation(
                "[Diagnostic]{Label} {Endpoint} completed in {Elapsed}ms",
                label, endpointName, sw.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogWarning(ex,
                "[Diagnostic]{Label} {Endpoint} threw after {Elapsed}ms",
                label, endpointName, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
