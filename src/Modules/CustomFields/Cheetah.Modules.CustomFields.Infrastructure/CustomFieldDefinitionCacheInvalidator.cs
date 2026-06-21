using Cheetah.Core.Cache;
using Cheetah.Core.Events;
using Cheetah.Modules.CustomFields.DomainEvents;
using Cheetah.Modules.CustomFields.Shared;

namespace Cheetah.Modules.CustomFields.Infrastructure;

/// <summary>
/// Сбрасывает кэш определений по событиям изменения/создания/деактивации определения. В
/// микросервисной топологии событие приходит по общей шине на все инстансы (eventually consistent).
/// </summary>
public sealed class CustomFieldDefinitionCacheInvalidator
    : IEventHandler<CustomFieldDefinitionCreatedIntegrationEvent>,
      IEventHandler<CustomFieldDefinitionChangedIntegrationEvent>,
      IEventHandler<CustomFieldDefinitionDeactivatedIntegrationEvent>
{
    private readonly ICacheService _cache;

    public CustomFieldDefinitionCacheInvalidator(ICacheService cache) => _cache = cache;

    public ValueTask HandleAsync(CustomFieldDefinitionCreatedIntegrationEvent @event, CancellationToken ct = default)
        => Invalidate(@event.TenantId, @event.EntityType, ct);

    public ValueTask HandleAsync(CustomFieldDefinitionChangedIntegrationEvent @event, CancellationToken ct = default)
        => Invalidate(@event.TenantId, @event.EntityType, ct);

    public ValueTask HandleAsync(CustomFieldDefinitionDeactivatedIntegrationEvent @event, CancellationToken ct = default)
        => Invalidate(@event.TenantId, @event.EntityType, ct);

    private async ValueTask Invalidate(Guid? tenantId, string entityType, CancellationToken ct)
    {
        // Сбрасываем ключ скоупа тенанта и глобальный — определения тенанта включают глобальные шаблоны.
        await _cache.RemoveAsync(CustomFieldsConstants.DefinitionsCacheKey(tenantId, entityType), ct);
        if (tenantId is not null)
            await _cache.RemoveAsync(CustomFieldsConstants.DefinitionsCacheKey(null, entityType), ct);
    }
}
