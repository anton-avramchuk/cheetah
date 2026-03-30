namespace Cheetah.Backend.Endpoints.Configuration;

/// <summary>
/// Fluent configuration for endpoints
/// </summary>
public sealed class EndpointConfiguration
{
    internal string? Name { get; private set; }
    internal string? Description { get; private set; }
    internal string? Summary { get; private set; }
    internal List<string> Tags { get; } = new();
    internal bool AllowAnonymous { get; private set; }
    internal List<string> AuthorizationPolicies { get; } = new();
    internal List<string> RequiredPermissions { get; } = new();
    internal bool IsDeprecated { get; private set; }
    internal BrowserCacheSettings? CacheControl { get; private set; }

    public EndpointConfiguration WithName(string name)
    {
        Name = name;
        return this;
    }

    public EndpointConfiguration WithDescription(string description)
    {
        Description = description;
        return this;
    }

    public EndpointConfiguration WithSummary(string summary)
    {
        Summary = summary;
        return this;
    }

    public EndpointConfiguration WithTags(params string[] tags)
    {
        Tags.AddRange(tags);
        return this;
    }

    public EndpointConfiguration AllowAnonymousAccess()
    {
        AllowAnonymous = true;
        return this;
    }

    public EndpointConfiguration RequireAuthorization(params string[] policies)
    {
        AuthorizationPolicies.AddRange(policies);
        return this;
    }

    public EndpointConfiguration RequirePermissions(params string[] permissions)
    {
        RequiredPermissions.AddRange(permissions);
        return this;
    }

    public EndpointConfiguration MarkAsDeprecated()
    {
        IsDeprecated = true;
        return this;
    }

    /// <summary>
    /// Sets Cache-Control response header for browser caching.
    /// Only meaningful for GET endpoints.
    /// </summary>
    /// <param name="maxAgeSeconds">Cache lifetime in seconds (e.g. 60, 3600).</param>
    /// <param name="isPublic">
    /// true → Cache-Control: public (allows CDN caching).
    /// false → Cache-Control: private (browser only).
    /// </param>
    public EndpointConfiguration WithCacheControl(int maxAgeSeconds, bool isPublic = false)
    {
        CacheControl = new BrowserCacheSettings(maxAgeSeconds, isPublic);
        return this;
    }

    /// <summary>
    /// Disables caching entirely: Cache-Control: no-store, no-cache.
    /// </summary>
    public EndpointConfiguration WithNoCache()
    {
        CacheControl = new BrowserCacheSettings(0, NoStore: true);
        return this;
    }
}
