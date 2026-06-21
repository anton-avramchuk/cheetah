using Cheetah.Core.Domain;
using Cheetah.Modules.CustomFields.DomainEvents;
using Cheetah.Modules.CustomFields.Shared;

namespace Cheetah.Modules.CustomFields.Domain.Entities;

/// <summary>
/// Определение кастомного поля для зарегистрированного типа сущности. Per-tenant
/// (<c>TenantId == null</c> — глобальный шаблон от OwnerService). Правила валидации хранятся
/// сериализованным JSON (<see cref="ValidationRulesJson"/>), видимость — JsonLogic-выражением.
/// </summary>
public sealed class CustomFieldDefinition : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    public Guid? TenantId { get; private set; }
    public string EntityType { get; private set; } = null!;   // "crm.deal" — зарегистрированный тип
    public string Key { get; private set; } = null!;          // уникален в (TenantId, EntityType)
    public string Label { get; private set; } = null!;
    public CustomFieldDataType DataType { get; private set; }
    public bool Required { get; private set; }
    public IReadOnlyList<string>? Options { get; private set; }       // для Enum/MultiEnum
    public string? ValidationRulesJson { get; private set; }          // List<IValidationRule> (jsonb)
    public string? VisibilityRule { get; private set; }               // JsonLogic над значениями+контекстом
    public int Order { get; private set; }
    public bool IsActive { get; private set; } = true;

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    private CustomFieldDefinition() { } // EF

    public static CustomFieldDefinition Create(Guid? tenantId, string entityType, string key,
        string label, CustomFieldDataType dataType, bool required,
        IReadOnlyList<string>? options, string? validationRulesJson, string? visibilityRule, int order)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityType);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        if (dataType is CustomFieldDataType.Enum or CustomFieldDataType.MultiEnum
            && (options is null || options.Count == 0))
            throw new InvalidOperationException("Enum/MultiEnum field requires non-empty Options.");

        var d = new CustomFieldDefinition
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EntityType = entityType.Trim(),
            Key = key.Trim(),
            Label = label,
            DataType = dataType,
            Required = required,
            Options = options,
            ValidationRulesJson = validationRulesJson,
            VisibilityRule = visibilityRule,
            Order = order,
            IsActive = true
        };
        d.AddDomainEvent(new CustomFieldDefinitionCreatedIntegrationEvent(d.Id, d.TenantId, d.EntityType, d.Key));
        return d;
    }

    /// <summary>Изменение метаданных. <c>DataType</c> неизменяем — защита значений в jsonb (план §1.2-3).</summary>
    public void UpdateMetadata(string label, bool required, IReadOnlyList<string>? options,
        string? validationRulesJson, string? visibilityRule, int order)
    {
        if (DataType is CustomFieldDataType.Enum or CustomFieldDataType.MultiEnum
            && (options is null || options.Count == 0))
            throw new InvalidOperationException("Enum/MultiEnum field requires non-empty Options.");

        Label = label;
        Required = required;
        Options = options;
        ValidationRulesJson = validationRulesJson;
        VisibilityRule = visibilityRule;
        Order = order;
        AddDomainEvent(new CustomFieldDefinitionChangedIntegrationEvent(Id, TenantId, EntityType, Key));
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
        AddDomainEvent(new CustomFieldDefinitionDeactivatedIntegrationEvent(Id, TenantId, EntityType, Key));
    }
}
