using Cheetah.Contracts.Responses;
using Cheetah.Modules.CustomFields.Shared;
using Cheetah.Validation;

namespace Cheetah.Modules.CustomFields.Contracts;

/// <summary>Определение кастомного поля (метаданные для админки/формы).</summary>
public sealed record CustomFieldDefinitionDto(
    Guid Id,
    Guid? TenantId,
    string EntityType,
    string Key,
    string Label,
    CustomFieldDataType DataType,
    bool Required,
    IReadOnlyList<string>? Options,
    IReadOnlyList<IValidationRule> ValidationRules,
    string? VisibilityRule,
    int Order,
    bool IsActive,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? UpdatedAt) : ICrmResponse;

/// <summary>Набор значений кастомных полей конкретной сущности.</summary>
public sealed record CustomFieldValuesDto(
    string EntityType,
    string EntityId,
    IReadOnlyDictionary<string, object?> Values) : ICrmResponse;

/// <summary>Зарегистрированный тип сущности (каталог расширяемых типов).</summary>
public sealed record CustomFieldEntityTypeDto(
    string Key,
    string DisplayName,
    string OwnerService,
    CustomFieldEntityIdType IdType,
    bool IsActive) : ICrmResponse;
