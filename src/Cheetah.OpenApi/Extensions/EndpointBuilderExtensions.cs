using Cheetah.OpenApi.Services;
using Microsoft.AspNetCore.Routing;

namespace Cheetah.OpenApi.Extensions;

public static class EndpointBuilderExtensions
{
    public static void UseCombinedOpenApi(this IEndpointRouteBuilder builder,
        string route = OpenApiConstants.OpenApiCombinedDocumentPath)
    {
        builder.MapGet(route, async (IOpenApiAggregator aggregator) =>
        {
            var doc = await aggregator.GetCombinedDocumentAsync();
            return Results.Json(doc);
        }).AllowAnonymous();
    }
}
