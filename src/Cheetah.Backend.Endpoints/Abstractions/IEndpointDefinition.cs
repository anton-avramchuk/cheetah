using Cheetah.Backend.Endpoints.Configuration;

namespace Cheetah.Backend.Endpoints.Abstractions;

/// <summary>
/// Base interface for all endpoint definitions
/// Pure metadata container - no mapping logic
/// Source generator will generate registration code
/// </summary>
public interface IEndpointDefinition
{
    /// <summary>
    /// Gets the HTTP method for this endpoint
    /// </summary>
    HttpMethod Method { get; }

    /// <summary>
    /// Gets the route pattern (e.g., "/api/tenants/{id:guid}")
    /// </summary>
    string Route { get; }

    /// <summary>
    /// Gets the endpoint name for route linking
    /// </summary>
    string? Name { get; }

    /// <summary>
    /// Gets the endpoint description for OpenAPI
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// Gets the summary for OpenAPI
    /// </summary>
    string? Summary { get; }

    /// <summary>
    /// Gets the tags for grouping in OpenAPI
    /// </summary>
    string[] Tags { get; }

    /// <summary>
    /// Gets whether this endpoint allows anonymous access
    /// </summary>
    bool AllowAnonymous { get; }

    /// <summary>
    /// Gets the authorization policies required
    /// </summary>
    string[] AuthorizationPolicies { get; }

    /// <summary>
    /// Gets the required permissions
    /// </summary>
    string[] RequiredPermissions { get; }

    /// <summary>
    /// Gets whether this endpoint is deprecated
    /// </summary>
    bool IsDeprecated { get; }

    /// <summary>
    /// Gets browser cache settings (Cache-Control header).
    /// null means no Cache-Control header is set.
    /// </summary>
    BrowserCacheSettings? CacheControl { get; }
}
