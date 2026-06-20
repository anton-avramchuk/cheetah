using Cheetah.FeatureManagement;

namespace Cheetah.Modules.FeatureManagement.Contracts;

/// <summary>Правило таргетинга на границе API (соответствует <c>TargetingRuleDefinition</c> движка).</summary>
public sealed record TargetingRuleDto(
    int Order,
    string FilterName,
    IReadOnlyDictionary<string, object?> Parameters,
    string? ResultVariant = null,
    bool Negate = false);

/// <summary>Вариант A/B на границе API.</summary>
public sealed record FeatureVariantDto(string Name, string? Value, int Weight);

/// <summary>Результат оценки флага (ответ <c>/evaluate</c>).</summary>
public sealed record FeatureEvaluationDto(bool Enabled, string? Variant);

/// <summary>
/// Дескриптор флага для реестра: сервис декларирует свои флаги при старте (как Permissions.Catalog/Tags).
/// Идемпотентный upsert метаданных — НЕ трогает ручные <c>Enabled</c>/таргетинг существующего флага.
/// </summary>
public sealed record FeatureDefinitionDescriptor(
    string Key,
    string Name,
    string OwnerService,
    FeatureValueType ValueType = FeatureValueType.Bool,
    string? Description = null,
    IReadOnlyList<string>? Variants = null);
