using Cheetah.FeatureManagement;

namespace Cheetah.Modules.FeatureManagement.Contracts;

/// <summary>
/// Базовый запрос создания флага. Абстрактен: наследник объявляет <c>sealed record</c> и добавляет
/// свои поля. Точка расширяемости входных контрактов.
/// </summary>
public abstract record CreateFeatureFlagRequestBase
{
    public string Key { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public string OwnerService { get; init; } = null!;
    public FeatureValueType ValueType { get; init; } = FeatureValueType.Bool;
    public IReadOnlyList<FeatureVariantDto> Variants { get; init; } = [];

    /// <summary>Ключ родительского флага (каскад) или <c>null</c> — флаг верхнего уровня.</summary>
    public string? ParentKey { get; init; }
}

/// <summary>Запрос смены родителя флага (каскад). <c>null</c> — снять родителя (сделать верхнего уровня).</summary>
public abstract record SetParentFeatureFlagRequestBase
{
    public string? ParentKey { get; init; }
}

/// <summary>Базовый запрос смены таргетинга (полная замена набора правил).</summary>
public abstract record SetTargetingRequestBase
{
    public IReadOnlyList<TargetingRuleDto> Rules { get; init; } = [];
}

/// <summary>Запрос override-а для тенанта.</summary>
public abstract record SetTenantOverrideRequestBase
{
    public bool Enabled { get; init; }
    public IReadOnlyList<TargetingRuleDto> Rules { get; init; } = [];
}
