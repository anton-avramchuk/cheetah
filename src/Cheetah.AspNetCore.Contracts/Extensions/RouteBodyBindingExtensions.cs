using System.Collections.Concurrent;
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

    private static readonly ConcurrentDictionary<Type, IEnumerable<(string Name, string RouteName)>> _routeBoundMembers = new();

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

        ApplyRouteValues<TRequest>(context, jsonObject);

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

        ApplyRouteValues<TRequest>(context, jsonObject);

        return jsonObject.Deserialize<TRequest>(_options)!;
    }

    /// <summary>
    /// Writes route values into <paramref name="jsonObject"/> for every member of
    /// <typeparamref name="TRequest"/> marked <see cref="FromRouteAttribute"/> — both
    /// positional record parameters and (possibly inherited) settable properties.
    /// </summary>
    private static void ApplyRouteValues<TRequest>(HttpContext context, JsonObject jsonObject)
    {
        foreach (var (name, routeName) in GetRouteBoundMembers(typeof(TRequest)))
        {
            var routeValue = context.Request.RouteValues[routeName];

            if (routeValue != null)
                jsonObject[name] = JsonValue.Create(routeValue.ToString());
        }
    }

    private static IEnumerable<(string Name, string RouteName)> GetRouteBoundMembers(Type type)
        => _routeBoundMembers.GetOrAdd(type, static t =>
        {
            var members = new List<(string, string)>();

            var ctor = t.GetConstructors().MaxBy(c => c.GetParameters().Length);

            if (ctor != null)
            {
                foreach (var param in ctor.GetParameters())
                {
                    if (param.Name == null)
                        continue;

                    if (param.GetCustomAttribute<FromRouteAttribute>() is { } fromRoute)
                        members.Add((param.Name, fromRoute.Name ?? param.Name));
                }
            }

            foreach (var property in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.GetCustomAttribute<FromRouteAttribute>() is not { } fromRoute)
                    continue;

                if (members.Exists(m => string.Equals(m.Item1, property.Name, StringComparison.OrdinalIgnoreCase)))
                    continue;

                members.Add((property.Name, fromRoute.Name ?? property.Name));
            }

            return members;
        });
}
