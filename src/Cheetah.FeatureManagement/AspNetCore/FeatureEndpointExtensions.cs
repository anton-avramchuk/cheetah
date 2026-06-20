using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.FeatureManagement.AspNetCore;

/// <summary>
/// Декларативная защита эндпоинта фич-флагом: <c>routes.MapGet(...).RequireFeature("deals.kanban-v2")</c>.
/// Если флаг выключен в контексте запроса — эндпоинт отвечает 404 (фича «не существует»).
/// </summary>
public static class FeatureEndpointExtensions
{
    public static TBuilder RequireFeature<TBuilder>(this TBuilder builder, string featureKey, int statusCode = StatusCodes.Status404NotFound)
        where TBuilder : IEndpointConventionBuilder
    {
        builder.AddEndpointFilter(async (ctx, next) =>
        {
            var sp = ctx.HttpContext.RequestServices;
            var manager = sp.GetRequiredService<IFeatureManager>();
            var contextBuilder = sp.GetRequiredService<IHttpFeatureContextBuilder>();

            var featureContext = contextBuilder.Build(ctx.HttpContext);
            if (!await manager.IsEnabledAsync(featureKey, featureContext, ctx.HttpContext.RequestAborted))
                return Results.StatusCode(statusCode);

            return await next(ctx);
        });
        return builder;
    }
}
