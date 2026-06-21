using Cheetah.Modules.CustomFields.Contracts;

namespace Cheetah.Modules.CustomFields.Client;

/// <summary>
/// HTTP-клиент к сервису CustomFields (server-to-server): регистрация расширяемых типов при старте,
/// чтение/запись значений и определений.
/// </summary>
public interface ICustomFieldsClient
{
    /// <summary>Идемпотентный upsert дескрипторов типов в каталог (registry/sync).</summary>
    ValueTask SyncTypesAsync(IReadOnlyList<CustomFieldEntityTypeDescriptor> descriptors, CancellationToken ct = default);

    /// <summary>Активные/все определения типа.</summary>
    ValueTask<IReadOnlyList<CustomFieldDefinitionDto>> ListDefinitionsAsync(
        string entityType, bool onlyActive = true, CancellationToken ct = default);

    /// <summary>Значения кастомных полей сущности.</summary>
    ValueTask<CustomFieldValuesDto> GetValuesAsync(string entityType, string entityId, CancellationToken ct = default);

    /// <summary>Значения для списка сущностей одного типа (анти-N+1).</summary>
    ValueTask<IReadOnlyDictionary<string, IReadOnlyDictionary<string, object?>>> BatchGetValuesAsync(
        string entityType, IReadOnlyList<string> entityIds, CancellationToken ct = default);

    /// <summary>Upsert значений сущности (с серверной валидацией).</summary>
    ValueTask SetValuesAsync(SetCustomFieldValuesRequest request, CancellationToken ct = default);
}
