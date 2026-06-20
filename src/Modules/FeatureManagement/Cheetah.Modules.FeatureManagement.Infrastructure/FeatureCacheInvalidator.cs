using Cheetah.Core.Cache;
using Cheetah.Core.Events;
using Cheetah.Modules.FeatureManagement.DomainEvents;

namespace Cheetah.Modules.FeatureManagement.Infrastructure;

/// <summary>
/// Сбрасывает кэш определений по событиям изменения флага. В микросервисной топологии событие
/// приходит по общей шине на все инстансы (eventually consistent).
/// <para>
/// MVP-замечание: глобальное изменение точечно удаляет global-ключ; tenant-специфичные записи без
/// собственного override догоняются по TTL (<see cref="Shared.FeatureManagementConstants.CacheTtl"/>).
/// Точная инвалидация всех тенантов — через generation-токен (follow-up).
/// </para>
/// </summary>
public sealed class FeatureCacheInvalidator
    : IEventHandler<FeatureFlagChangedIntegrationEvent>, IEventHandler<FeatureFlagToggledIntegrationEvent>
{
    private readonly ICacheService _cache;

    public FeatureCacheInvalidator(ICacheService cache) => _cache = cache;

    public async ValueTask HandleAsync(FeatureFlagChangedIntegrationEvent @event, CancellationToken ct = default)
    {
        await _cache.RemoveAsync(FeatureCacheKeys.Definition(@event.Key, null), ct);
        if (@event.TenantId is { } tenantId)
            await _cache.RemoveAsync(FeatureCacheKeys.Definition(@event.Key, tenantId), ct);
    }

    public ValueTask HandleAsync(FeatureFlagToggledIntegrationEvent @event, CancellationToken ct = default)
        => _cache.RemoveAsync(FeatureCacheKeys.Definition(@event.Key, null), ct);
}
