using System.Text.Json.Nodes;

namespace Cheetah.OpenApi.Services;

public interface IOpenApiAggregator
{
    Task<JsonObject> GetCombinedDocumentAsync();
}
