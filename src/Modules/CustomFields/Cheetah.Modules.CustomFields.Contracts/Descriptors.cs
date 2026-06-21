using Cheetah.Modules.CustomFields.Shared;

namespace Cheetah.Modules.CustomFields.Contracts;

/// <summary>
/// Дескриптор расширяемого типа сущности — сервисы декларируют его при старте через Client
/// (идемпотентный upsert в каталог, как Tags / Permissions.Catalog).
/// </summary>
public sealed record CustomFieldEntityTypeDescriptor(
    string Key,
    string DisplayName,
    string OwnerService,
    CustomFieldEntityIdType IdType,
    IReadOnlyList<PredefinedFieldDescriptor>? PredefinedFields = null);

/// <summary>Предопределённое поле «из коробки» (регистрируется как глобальное определение, TenantId == null).</summary>
public sealed record PredefinedFieldDescriptor(
    string Key,
    string Label,
    CustomFieldDataType DataType,
    bool Required = false,
    IReadOnlyList<string>? Options = null,
    int Order = 0);
