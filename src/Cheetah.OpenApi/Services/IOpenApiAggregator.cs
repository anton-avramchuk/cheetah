using Microsoft.OpenApi;

namespace Cheetah.OpenApi.Services;

public interface IOpenApiAggregator
{
    Task<OpenApiDocument> GetCombinedOpenApiDocumentAsync();
}