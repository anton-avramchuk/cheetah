using Cheetah.Core.Cache;
using Cheetah.Core.CQRS;
using Cheetah.Core.DependencyInjection;
using Cheetah.Features.Application.Queries;

namespace Cheetah.Features.Application.Services;

[Export(LifetimeType.Scoped, typeof(IFeatureChecker))]
public class FeatureChecker(
    IDispatcher dispatcher,
    ICacheService cacheService
) : IFeatureChecker
{
    private const string CacheKeyPrefix = "Feature:";
    private const int CacheExpirationMinutes = 15;

    public async ValueTask<bool> IsEnabledAsync(string featureId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CacheKeyPrefix}{tenantId}:{featureId}";

        // Try get from cache
        var cachedValue = await cacheService.GetAsync<bool?>(cacheKey, cancellationToken);
        if (cachedValue.HasValue)
            return cachedValue.Value;

        // Query from database
        var query = new CheckFeatureQuery(tenantId, featureId);
        var isEnabled = await dispatcher.QueryAsync<CheckFeatureQuery, bool>(query, cancellationToken);

        // Cache result
        await cacheService.SetAsync(cacheKey, isEnabled, TimeSpan.FromMinutes(CacheExpirationMinutes), cancellationToken);

        return isEnabled;
    }

    public async ValueTask RequireAsync(string featureId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var isEnabled = await IsEnabledAsync(featureId, tenantId, cancellationToken);
        if (!isEnabled)
            throw new InvalidOperationException($"Feature '{featureId}' is not enabled for tenant {tenantId}");
    }
}
