using Cheetah.Core.Domain;
using Cheetah.Modules.CustomFields.DomainEvents;

namespace Cheetah.Modules.CustomFields.Domain.Entities;

/// <summary>
/// Набор значений кастомных полей одной сущности. Один <c>jsonb</c>-набор на
/// <c>(TenantId, EntityType, EntityId)</c> — атомарный upsert, без EAV-джойнов, GIN-индексация.
/// </summary>
public sealed class CustomFieldValueSet : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public Guid? TenantId { get; private set; }
    public string EntityType { get; private set; } = null!;
    public string EntityId { get; private set; } = null!;   // строкой, канонично
    public string ValuesJson { get; private set; } = "{}";   // jsonb {fieldKey: value}

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    private CustomFieldValueSet() { } // EF

    public static CustomFieldValueSet Create(Guid? tenantId, string entityType, string entityId, string valuesJson)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
        ArgumentException.ThrowIfNullOrWhiteSpace(entityId);
        var set = new CustomFieldValueSet
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EntityType = entityType.Trim(),
            EntityId = entityId.Trim(),
            ValuesJson = valuesJson
        };
        set.AddDomainEvent(new CustomFieldValuesChangedIntegrationEvent(tenantId, set.EntityType, set.EntityId));
        return set;
    }

    /// <summary>Заменяет весь набор значений (валидация выполняется в Application до вызова).</summary>
    public void Replace(string valuesJson)
    {
        ValuesJson = valuesJson;
        AddDomainEvent(new CustomFieldValuesChangedIntegrationEvent(TenantId, EntityType, EntityId));
    }
}
