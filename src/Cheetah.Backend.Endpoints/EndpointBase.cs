using Cheetah.Contracts.Requests;
using Cheetah.Contracts.Responses;
using Cheetah.Backend.Endpoints.Abstractions;
using Cheetah.Backend.Endpoints.Configuration;

namespace Cheetah.Backend.Endpoints;

/// <summary>
/// Base class for all CRM endpoints with request/response
/// Pure metadata container - no mapping logic
/// Source generator will create registration code
/// </summary>
public abstract class EndpointBase<TRequest, TResponse> : ICrmEndpoint<TRequest, TResponse>
    where TRequest : ICrmRequest
    where TResponse : ICrmResponse
{
    private EndpointConfiguration? _configuration;

    public abstract HttpMethod Method { get; }
    public abstract string Route { get; }

    public virtual string? Name => GetConfiguration().Name;
    public virtual string? Description => GetConfiguration().Description;
    public virtual string? Summary => GetConfiguration().Summary;
    public virtual string[] Tags => GetConfiguration().Tags.ToArray();
    public virtual bool AllowAnonymous => GetConfiguration().AllowAnonymous;
    public virtual string[] AuthorizationPolicies => GetConfiguration().AuthorizationPolicies.ToArray();
    public virtual string[] RequiredPermissions => GetConfiguration().RequiredPermissions.ToArray();
    public virtual bool IsDeprecated => GetConfiguration().IsDeprecated;
    public virtual BrowserCacheSettings? CacheControl => GetConfiguration().CacheControl;

    /// <summary>
    /// Configure endpoint metadata
    /// Called by source generator during initialization
    /// </summary>
    protected virtual void Configure(EndpointConfiguration config)
    {
        // Override to configure
    }

    /// <summary>
    /// Gets the configuration, initializing it if needed
    /// </summary>
    private EndpointConfiguration GetConfiguration()
    {
        if (_configuration == null)
        {
            _configuration = new EndpointConfiguration();
            Configure(_configuration);
        }
        return _configuration;
    }
}
