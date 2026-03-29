using Cheetah.Core.Diagnostic.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cheetah.Core.Diagnostic.AspNetCore.Middleware;

public class RequestLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestLoggingMiddleware> logger,
    IOptions<DiagnosticsOptions> options)
{
    private readonly DiagnosticsOptions _options = options.Value;

    public async Task InvokeAsync(HttpContext context)
    {
        var opts = _options.RequestLogging;

        if (!_options.Enabled || !opts.Enabled || IsExcluded(context.Request.Path, opts))
        {
            await next(context);
            return;
        }

        var body = await ReadBodyAsync(context.Request, opts);
        var query = opts.LogQueryString ? context.Request.QueryString.ToString() : string.Empty;

        logger.LogInformation(
            "HTTP {Method} {Path}{Query} | Body: {Body}",
            context.Request.Method,
            context.Request.Path,
            query,
            body);

        await next(context);

        if (opts.LogResponseStatus)
        {
            logger.LogInformation(
                "HTTP {Method} {Path} => {StatusCode}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode);
        }
    }

    private static bool IsExcluded(PathString path, RequestLoggingOptions opts)
        => opts.ExcludePaths.Any(p => path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase));

    private static async Task<string> ReadBodyAsync(HttpRequest request, RequestLoggingOptions opts)
    {
        if (!opts.LogBody || (request.ContentLength is null or 0 && !request.HasFormContentType))
            return "(empty)";

        request.EnableBuffering();

        var bytesToRead = (int)Math.Min(
            opts.MaxBodyBytes,
            request.ContentLength ?? opts.MaxBodyBytes);

        using var reader = new StreamReader(request.Body, leaveOpen: true);
        var buffer = new char[bytesToRead];
        var read = await reader.ReadBlockAsync(buffer, 0, bytesToRead);
        request.Body.Position = 0;

        if (read == 0) return "(empty)";

        var text = new string(buffer, 0, read);

        return request.ContentLength > opts.MaxBodyBytes
            ? $"{text}... [truncated — {request.ContentLength} bytes total]"
            : text;
    }
}
