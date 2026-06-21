using Cheetah.Modules.CustomFields.Shared;

namespace Cheetah.Modules.CustomFields.Domain.Abstractions;

/// <summary>
/// Плоский снимок определения поля для горячего пути (валидация/видимость) и кэша.
/// Без доменных событий — сериализуем для распределённого кэша. Правила валидации остаются
/// сырым JSON (<see cref="ValidationRulesJson"/>), их разбор — задача Application.
/// </summary>
public sealed record CustomFieldDefinitionSnapshot(
    Guid Id,
    Guid? TenantId,
    string EntityType,
    string Key,
    string Label,
    CustomFieldDataType DataType,
    bool Required,
    IReadOnlyList<string>? Options,
    string? ValidationRulesJson,
    string? VisibilityRule,
    int Order);

/// <summary>
/// Порт чтения активных определений типа с кэшем (cache-aside). Реализуется Infrastructure поверх
/// репозитория + <c>ICacheService</c>. Определения меняются редко, читаются на каждый показ/валидацию.
/// </summary>
public interface ICustomFieldDefinitionReader
{
    /// <summary>Активные определения типа в скоупе тенанта (включая глобальные шаблоны), упорядочены по Order.</summary>
    ValueTask<IReadOnlyList<CustomFieldDefinitionSnapshot>> GetActiveAsync(
        Guid? tenantId, string entityType, CancellationToken ct = default);
}
