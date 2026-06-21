using Cheetah.Contracts.Attributes;
using Cheetah.Contracts.Requests;
using Cheetah.Modules.CustomFields.Shared;
using Cheetah.Validation;

namespace Cheetah.Modules.CustomFields.Contracts;

// ── Определения ──────────────────────────────────────────────────────────────────────────

/// <summary>Создание определения кастомного поля для зарегистрированного типа сущности.</summary>
public sealed record CreateCustomFieldDefinitionRequest(
    string EntityType,
    string Key,
    string Label,
    CustomFieldDataType DataType = CustomFieldDataType.String,
    bool Required = false,
    IReadOnlyList<string>? Options = null,
    IReadOnlyList<IValidationRule>? ValidationRules = null,
    string? VisibilityRule = null,
    int Order = 0) : ICrmRequest;

/// <summary>Изменение метаданных определения (<c>DataType</c> неизменяем после создания).</summary>
public sealed record UpdateCustomFieldDefinitionRequest(
    [property: FromRoute] Guid Id,
    string Label,
    bool Required = false,
    IReadOnlyList<string>? Options = null,
    IReadOnlyList<IValidationRule>? ValidationRules = null,
    string? VisibilityRule = null,
    int Order = 0) : ICrmRequest;

// ── Значения ─────────────────────────────────────────────────────────────────────────────

/// <summary>Upsert набора значений кастомных полей сущности (с валидацией).</summary>
public sealed record SetCustomFieldValuesRequest(
    string EntityType,
    string EntityId,
    IReadOnlyDictionary<string, object?> Values) : ICrmRequest;

/// <summary>Батч-чтение значений для списка сущностей одного типа (анти-N+1).</summary>
public sealed record BatchGetValuesRequest(
    string EntityType,
    IReadOnlyList<string> EntityIds) : ICrmRequest;

/// <summary>Запрос видимых полей с учётом текущих значений + контекста (JsonLogic).</summary>
public sealed record GetVisibleFieldsRequest(
    string EntityType,
    string EntityId,
    IReadOnlyDictionary<string, object?>? Context = null) : ICrmRequest;
