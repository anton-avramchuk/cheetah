using Cheetah.Core.Cache;
using Cheetah.Core.DataAccess.Abstractions;
using Cheetah.Core.DependencyInjection;
using Cheetah.Modules.CustomFields.Domain.Abstractions;
using Cheetah.Modules.CustomFields.Domain.Entities;
using Cheetah.Modules.CustomFields.Domain.Specifications;
using Cheetah.Modules.CustomFields.Shared;

namespace Cheetah.Modules.CustomFields.Infrastructure;

/// <summary>
/// Реализация порта чтения определений с cache-aside: горячий путь (валидация/видимость) читает из
/// <see cref="ICacheService"/>, БД — только на miss. Инвалидация — по событиям изменения определений
/// (<see cref="CustomFieldDefinitionCacheInvalidator"/>) на всех инстансах через шину.
/// </summary>
[Export(LifetimeType.Scoped, typeof(ICustomFieldDefinitionReader))]
public sealed class CachedCustomFieldDefinitionReader : ICustomFieldDefinitionReader
{
    private readonly IRepository<CustomFieldDefinition, Guid> _definitions;
    private readonly ICacheService _cache;

    public CachedCustomFieldDefinitionReader(
        IRepository<CustomFieldDefinition, Guid> definitions, ICacheService cache)
    {
        _definitions = definitions;
        _cache = cache;
    }

    public ValueTask<IReadOnlyList<CustomFieldDefinitionSnapshot>> GetActiveAsync(
        Guid? tenantId, string entityType, CancellationToken ct = default)
        => _cache.GetOrSetAsync(
            CustomFieldsConstants.DefinitionsCacheKey(tenantId, entityType),
            async () => await LoadAsync(tenantId, entityType, ct),
            CustomFieldsConstants.DefinitionsCacheTtl,
            ct);

    private async ValueTask<IReadOnlyList<CustomFieldDefinitionSnapshot>> LoadAsync(
        Guid? tenantId, string entityType, CancellationToken ct)
    {
        var defs = await _definitions.GetAllAsync(
            new DefinitionsByEntityTypeSpecification(tenantId, entityType, onlyActive: true), ct);

        return defs
            .OrderBy(d => d.Order)
            .Select(d => new CustomFieldDefinitionSnapshot(
                d.Id, d.TenantId, d.EntityType, d.Key, d.Label, d.DataType, d.Required,
                d.Options, d.ValidationRulesJson, d.VisibilityRule, d.Order))
            .ToArray();
    }
}
