using Cheetah.Core.Domain;
using Cheetah.FeatureManagement;
using Cheetah.Modules.FeatureManagement.DomainEvents;

namespace Cheetah.Modules.FeatureManagement.Domain.Entities;

/// <summary>
/// Абстрактный базовый агрегат флага. Шаблонный модуль не инстанцирует его сам — наследник
/// объявляет <c>sealed class FeatureFlag : FeatureFlagBase</c> со своей фабрикой (через
/// <see cref="InitializeCore"/>) и доп. полями (например, <c>JiraTicket</c>, <c>OwnerTeam</c>).
/// Это структурная точка расширяемости; поведенческая — plugin-фильтры <c>IFeatureFilter</c>.
/// </summary>
public abstract class FeatureFlagBase : AggregateRoot<Guid>, ICreateAtEntity, IUpdatedAtEntity
{
    private readonly List<TargetingRule> _rules = new();
    private readonly List<FeatureVariantDef> _variants = new();
    private readonly List<TenantOverride> _overrides = new();

    /// <summary>Стабильный публичный контракт <c>"{service}.{feature}"</c>.</summary>
    public string Key { get; protected set; } = null!;
    public string Name { get; protected set; } = null!;
    public string? Description { get; protected set; }
    public string OwnerService { get; protected set; } = null!;

    /// <summary>Kill-switch: мгновенно выключает фичу глобально, минуя весь таргетинг.</summary>
    public bool Enabled { get; protected set; }

    public FeatureValueType ValueType { get; protected set; }
    public bool IsActive { get; protected set; } = true;

    public IReadOnlyList<TargetingRule> Rules => _rules;
    public IReadOnlyList<FeatureVariantDef> Variants => _variants;
    public IReadOnlyList<TenantOverride> Overrides => _overrides;

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }

    protected FeatureFlagBase() { } // EF + наследник

    /// <summary>
    /// Заводит инварианты нового флага и событие создания. Вызывается фабрикой наследника
    /// (замена <c>new</c> абстрактной сущности). По умолчанию флаг выключен.
    /// </summary>
    protected void InitializeCore(Guid id, string key, string name, string ownerService,
        FeatureValueType valueType, string? description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerService);

        Id = id;
        Key = key.Trim();
        Name = name.Trim();
        OwnerService = ownerService.Trim();
        ValueType = valueType;
        Description = description;
        Enabled = false;
        IsActive = true;

        AddDomainEvent(new FeatureFlagCreatedIntegrationEvent(Key, OwnerService));
    }

    /// <summary>
    /// Идемпотентное обновление метаданных из реестра (registry/sync). НЕ трогает <see cref="Enabled"/>
    /// и таргетинг — чтобы рестарт сервиса не сбрасывал ручные настройки админа. Событий не публикует.
    /// </summary>
    public virtual void RegisterMetadata(string name, string? description, FeatureValueType valueType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
        Description = description;
        ValueType = valueType;
    }

    public virtual void Enable()
    {
        if (Enabled) return;
        Enabled = true;
        AddDomainEvent(new FeatureFlagToggledIntegrationEvent(Key, true));
    }

    public virtual void Disable()
    {
        if (!Enabled) return;
        Enabled = false;
        AddDomainEvent(new FeatureFlagToggledIntegrationEvent(Key, false));
    }

    /// <summary>Полная замена набора правил таргетинга. Публикует <c>Changed</c> (инвалидация кэша).</summary>
    public virtual void SetTargeting(IEnumerable<TargetingRule> rules)
    {
        _rules.Clear();
        _rules.AddRange(rules.OrderBy(r => r.Order));
        AddDomainEvent(new FeatureFlagChangedIntegrationEvent(Key, null));
    }

    /// <summary>Полная замена вариантов A/B. Публикует <c>Changed</c>.</summary>
    public virtual void SetVariants(IEnumerable<FeatureVariantDef> variants)
    {
        _variants.Clear();
        _variants.AddRange(variants);
        AddDomainEvent(new FeatureFlagChangedIntegrationEvent(Key, null));
    }

    /// <summary>Устанавливает/заменяет override для тенанта. Публикует <c>Changed</c> по тенанту.</summary>
    public void SetTenantOverride(TenantOverride ov)
    {
        _overrides.RemoveAll(o => o.TenantId == ov.TenantId);
        _overrides.Add(ov);
        AddDomainEvent(new FeatureFlagChangedIntegrationEvent(Key, ov.TenantId));
    }

    public void RemoveTenantOverride(Guid tenantId)
    {
        if (_overrides.RemoveAll(o => o.TenantId == tenantId) > 0)
            AddDomainEvent(new FeatureFlagChangedIntegrationEvent(Key, tenantId));
    }

    public virtual void Deactivate() => IsActive = false;
    public virtual void Activate() => IsActive = true;
}
