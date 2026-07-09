using Cheetah.FeatureManagement;
using Cheetah.FeatureManagement.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Cheetah.Backend.Endpoints.Http;

/// <summary>
/// IEndpointFilter фич-гейта декларативных эндпоинтов: выключенный/незарегистрированный флаг → 404
/// («фича не существует»). Регистрируется Source Generator'ом, если в EndpointConfiguration вызван
/// RequireFeature(...).
///
/// Если система флагов не подключена (нет <see cref="IFeatureDefinitionProvider"/>) — fail-open,
/// эндпоинт работает как раньше (тот же подход, что у <see cref="RateLimitFilter"/>). Когда флаги
/// подключены — семантика fail-closed: незарегистрированный флаг считается выключенным.
/// </summary>
public sealed class FeatureGateFilter : IEndpointFilter
{
    private readonly string _featureKey;

    public FeatureGateFilter(string featureKey) => _featureKey = featureKey;

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var http = context.HttpContext;
        var services = http.RequestServices;

        // Провайдер определений — признак «флаги подключены»; без него IFeatureManager неразрешим.
        if (services.GetService<IFeatureDefinitionProvider>() is null)
            return await next(context);

        var manager = services.GetRequiredService<IFeatureManager>();
        var contextBuilder = services.GetService<IHttpFeatureContextBuilder>();
        var featureContext = contextBuilder?.Build(http) ?? FeatureContext.Empty;

        if (!await manager.IsEnabledAsync(_featureKey, featureContext, http.RequestAborted))
            return Results.NotFound();

        return await next(context);
    }
}
