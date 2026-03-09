using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Cheetah.Contracts.Attributes;
using Microsoft.AspNetCore.Http;

namespace Cheetah.AspNetCore.Contracts.Extensions;

/// <summary>
/// Extension methods for binding requests that mix route parameters with a JSON body.
/// Used by generated update/patch endpoints where the request record has both
/// [FromRoute] constructor parameters (e.g. Id) and body properties.
/// </summary>
public static class RouteBodyBindingExtensions
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Deserializes the request body as JSON and injects route values for any
    /// constructor parameters decorated with <see cref="FromRouteAttribute"/>.
    /// </summary>
    public static async ValueTask<TRequest?> BindBodyWithRouteAsync<TRequest>(
        this HttpContext context, CancellationToken ct = default)
    {
        JsonObject jsonObject;

        if (context.Request.ContentLength > 0 || context.Request.Body.CanRead)
        {
            var node = await JsonSerializer.DeserializeAsync<JsonNode>(
                context.Request.Body, _options, ct);
            jsonObject = node?.AsObject() ?? new JsonObject();
        }
        else
        {
            jsonObject = new JsonObject();
        }

        // Inject route values for constructor parameters marked [FromRoute]
        var ctor = typeof(TRequest).GetConstructors()
            .MaxBy(c => c.GetParameters().Length);

        if (ctor != null)
        {
            foreach (var param in ctor.GetParameters())
            {
                var fromRoute = param.GetCustomAttribute(typeof(FromRouteAttribute));
                if (fromRoute == null || param.Name == null)
                    continue;

                var routeName = (fromRoute as FromRouteAttribute)?.Name ?? param.Name;
                var routeValue = context.Request.RouteValues[routeName];

                if (routeValue != null)
                    jsonObject[param.Name] = JsonValue.Create(routeValue.ToString());
            }
        }

        return jsonObject.Deserialize<TRequest>(_options);
    }

    /// <summary>
    /// Injects route values into an already body-bound request for any
    /// constructor parameters decorated with <see cref="FromRouteAttribute"/>.
    /// Use this when the endpoint uses <c>[FromBody]</c> (so OpenAPI/Scalar can
    /// generate an example body) but also needs route values merged in.
    /// </summary>
    public static TRequest MergeRouteValuesInto<TRequest>(this HttpContext context, TRequest bodyRequest)
    {
        var jsonObject = JsonSerializer.SerializeToNode(bodyRequest, _options)?.AsObject() ?? new JsonObject();

        var ctor = typeof(TRequest).GetConstructors()
            .MaxBy(c => c.GetParameters().Length);

        if (ctor != null)
        {
            foreach (var param in ctor.GetParameters())
            {
                var fromRoute = param.GetCustomAttribute(typeof(FromRouteAttribute));
                if (fromRoute == null || param.Name == null)
                    continue;

                var routeName = (fromRoute as FromRouteAttribute)?.Name ?? param.Name;
                var routeValue = context.Request.RouteValues[routeName];

                if (routeValue != null)
                    jsonObject[param.Name] = JsonValue.Create(routeValue.ToString());
            }
        }

        return jsonObject.Deserialize<TRequest>(_options)!;
    }
}
