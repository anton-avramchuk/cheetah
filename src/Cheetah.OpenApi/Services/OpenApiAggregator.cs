using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;

namespace Cheetah.OpenApi.Services;

public class OpenApiAggregator(
    IHttpClientFactory clientFactory,
    IClusterAddressProvider addressProvider,
    ILogger<OpenApiAggregator> logger)
    : IOpenApiAggregator
{
    public async Task<JsonObject> GetCombinedDocumentAsync()
    {
        var combined = new JsonObject
        {
            ["openapi"] = "3.1.1",
            ["info"] = new JsonObject { ["title"] = "Combined API", ["version"] = "v1" },
            ["paths"] = new JsonObject(),
            ["components"] = new JsonObject { ["schemas"] = new JsonObject() },
        };

        var combinedPaths   = combined["paths"]!.AsObject();
        var combinedSchemas = combined["components"]!["schemas"]!.AsObject();
        var securitySchemes = new JsonObject();

        foreach (var (address, routePrefix) in addressProvider.GetClusterAddresses())
        {
            JsonObject? doc;
            try
            {
                var uri  = new Uri(new Uri(address), "openapi/v1.json");
                var json = await clientFactory.CreateClient().GetStringAsync(uri);
                doc = JsonNode.Parse(json)?.AsObject();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to fetch OpenAPI from {Address}", address);
                continue;
            }

            if (doc is null) continue;

            // Determine the base path to strip from downstream paths
            // e.g. address "http://svc:8080/api" → strip "/api"
            var addressBasePath = "";
            if (Uri.TryCreate(address.TrimEnd('/'), UriKind.Absolute, out var addressUri))
            {
                addressBasePath = addressUri.AbsolutePath.TrimEnd('/');
                if (addressBasePath == "/") addressBasePath = "";
            }

            // Merge paths
            if (doc["paths"]?.AsObject() is { } paths)
            {
                foreach (var (rawKey, pathValue) in paths)
                {
                    var key = rawKey;
                    if (!string.IsNullOrEmpty(addressBasePath)
                        && key.StartsWith(addressBasePath, StringComparison.OrdinalIgnoreCase))
                        key = key[addressBasePath.Length..];

                    var newKey = $"{routePrefix.TrimEnd('/')}{key}";
                    combinedPaths[newKey] = pathValue?.DeepClone();
                }
            }

            // Merge component schemas (preserves example, description, etc.)
            if (doc["components"]?["schemas"]?.AsObject() is { } schemas)
            {
                foreach (var (schemaKey, schemaValue) in schemas)
                    combinedSchemas.TryAdd(schemaKey, schemaValue?.DeepClone());
            }

            // Merge security schemes
            if (doc["components"]?["securitySchemes"]?.AsObject() is { } schemes)
            {
                foreach (var (schemeKey, schemeValue) in schemes)
                    securitySchemes.TryAdd(schemeKey, schemeValue?.DeepClone());
            }
        }

        if (securitySchemes.Count > 0)
            combined["components"]!.AsObject()["securitySchemes"] = securitySchemes;

        logger.LogInformation("Aggregation complete: {Paths} paths, {Schemas} schemas",
            combinedPaths.Count, combinedSchemas.Count);

        return combined;
    }
}
