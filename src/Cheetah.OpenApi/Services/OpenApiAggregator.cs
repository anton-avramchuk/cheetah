using Microsoft.OpenApi;
using Microsoft.OpenApi.Readers;

namespace Cheetah.OpenApi.Services;

public class OpenApiAggregator(IHttpClientFactory clientFactory, IClusterAddressProvider addressProvider)
    : IOpenApiAggregator
{
    public async Task<OpenApiDocument> GetCombinedOpenApiDocumentAsync()
    {
        var clusterAddresses = addressProvider.GetClusterAddresses();

        var reader = new OpenApiStringReader();
        var combined = new OpenApiDocument
        {
            Info = new OpenApiInfo { Title = "Combined API", Version = "v1" },
            Paths = new OpenApiPaths()
        };

        foreach (var (address, routePrefix) in clusterAddresses)
        {
            try
            {
                
                var fullUri = new Uri(new Uri(address), "openapi/v1.json");
                var json = await clientFactory.CreateClient().GetStringAsync(fullUri);
                
               // reader.Read(json,out var apiDiagnostic);
                
                //var doc = reader.Read(json, out var diagnostic);

                // foreach (var path in doc.Paths)
                // {
                //     var newPathKey = $"{routePrefix.TrimEnd('/')}{path.Key}";
                //     combined.Paths[newPathKey] = path.Value;
                // }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке {address}: {ex.Message}");
            }
        }

        return combined;
    }
}