using Cheetah.Core.Diagnostic.Options;

namespace Cheetah.Core.Diagnostic.AspNetCore.Options;

public class RequestLoggingOptions
{
    /// <summary>Sub-section name inside <see cref="DiagnosticsOptions.SectionName"/>.</summary>
    public const string SubSection = "RequestLogging";

    /// <summary>
    /// Enable or disable HTTP request/response logging.
    /// Independent of the global <see cref="DiagnosticsOptions.Enabled"/> switch —
    /// both must be <c>true</c> for logging to occur.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Include the request body in the log message.
    /// Disable for binary uploads or endpoints with sensitive payloads.
    /// </summary>
    public bool LogBody { get; set; } = true;

    /// <summary>
    /// Maximum number of bytes of the request body to include in the log.
    /// Bodies larger than this limit are truncated and marked with [truncated].
    /// Default: 4096 bytes.
    /// </summary>
    public int MaxBodyBytes { get; set; } = 4096;

    /// <summary>
    /// Include the query string in the log message.
    /// Disable when query parameters may contain sensitive data (tokens, passwords).
    /// </summary>
    public bool LogQueryString { get; set; } = true;

    /// <summary>
    /// Log the response status code after the handler completes.
    /// </summary>
    public bool LogResponseStatus { get; set; } = true;

    /// <summary>
    /// Path prefixes that will be silently skipped.
    /// Matching is prefix-based: "/health" also suppresses "/health/live", "/health/ready", etc.
    /// Example: ["/health", "/metrics", "/swagger"]
    /// </summary>
    public List<string> ExcludePaths { get; set; } = [];
}
