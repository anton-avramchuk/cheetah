using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;

namespace Cheetah.OpenApi.Services;

public class OpenApiAggregator(
    IHttpClientFactory clientFactory,
    IClusterAddressProvider addressProvider,
    ILogger<OpenApiAggregator> logger)
    : IOpenApiAggregator
{
    public async Task<OpenApiDocument> GetCombinedOpenApiDocumentAsync()
    {
        logger.LogInformation("Starting OpenAPI aggregation...");

        var clusterAddresses = addressProvider.GetClusterAddresses().ToList();
        logger.LogInformation("Found {Count} cluster addresses", clusterAddresses.Count);

        foreach (var (addr, prefix) in clusterAddresses)
        {
            logger.LogInformation("  - Address: {Address}, Prefix: {Prefix}", addr, prefix);
        }

        var combined = new OpenApiDocument
        {
            Info = new OpenApiInfo { Title = "Combined API", Version = "v1" },
            Paths = new OpenApiPaths(),
            Components = new OpenApiComponents()
        };

        foreach (var (address, routePrefix) in clusterAddresses)
        {
            try
            {
                var fullUri = new Uri(new Uri(address), "openapi/v1.json");
                logger.LogInformation("Fetching OpenAPI from {Uri}", fullUri);

                var json = await clientFactory.CreateClient().GetStringAsync(fullUri);
                logger.LogInformation("Received {Length} bytes, loading OpenAPI document...", json.Length);

                using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
                var result = await OpenApiDocument.LoadAsync(stream);
                var doc = result.Document;

                if (result.Diagnostic?.Errors?.Count > 0)
                {
                    foreach (var error in result.Diagnostic.Errors)
                    {
                        logger.LogWarning("OpenAPI parse error: {Error}", error.Message);
                    }
                }

                logger.LogInformation("Document loaded. Paths count: {PathCount}", doc?.Paths?.Count ?? 0);

                if (doc?.Paths != null)
                {
                    // Determine the address base path to strip from downstream paths
                    var addressBasePath = "";
                    try
                    {
                        var addressUri = new Uri(address.TrimEnd('/') + "/");
                        addressBasePath = addressUri.AbsolutePath.TrimEnd('/');
                        if (addressBasePath == "/") addressBasePath = "";
                    }
                    catch
                    {
                        // ignored
                    }

                    foreach (var path in doc.Paths)
                    {
                        var pathKey = path.Key;

                        // Strip the address base path from downstream path
                        // e.g. address "http://svc/api" → strip "/api" from "/api/candidates" → "/candidates"
                        if (!string.IsNullOrEmpty(addressBasePath)
                            && pathKey.StartsWith(addressBasePath, StringComparison.OrdinalIgnoreCase))
                        {
                            pathKey = pathKey[addressBasePath.Length..];
                        }

                        var newPathKey = $"{routePrefix.TrimEnd('/')}{pathKey}";
                        combined.Paths[newPathKey] = path.Value;
                        logger.LogDebug("Added path: {Path}", newPathKey);
                    }
                }

                // Merge components (schemas, etc.)
                if (doc?.Components?.Schemas != null && combined.Components?.Schemas != null)
                {
                    logger.LogInformation("Merging {Count} schemas", doc.Components.Schemas.Count);
                    foreach (var schema in doc.Components.Schemas)
                    {
                        combined.Components.Schemas.TryAdd(schema.Key, schema.Value);
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "HTTP error fetching OpenAPI from {Address}: {Message}", address, ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing {Address}: {Message}", address, ex.Message);
            }
        }

        logger.LogInformation("Aggregation complete. Total paths: {Count}", combined.Paths.Count);
        return combined;
    }
}