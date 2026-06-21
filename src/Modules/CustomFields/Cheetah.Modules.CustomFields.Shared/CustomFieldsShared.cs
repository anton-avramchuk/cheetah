namespace Cheetah.Modules.CustomFields.Shared;

/// <summary>Тип данных кастомного поля. Определяет, как хранить/валидировать/рендерить значение.</summary>
public enum CustomFieldDataType
{
    String = 0,
    Text = 1,
    Number = 2,
    Date = 3,
    DateTime = 4,
    Bool = 5,
    Enum = 6,
    MultiEnum = 7,
    Reference = 8
}

/// <summary>Тип идентификатора сущности-потребителя — как кастовать/валидировать <c>EntityId</c>.</summary>
public enum CustomFieldEntityIdType
{
    Guid = 0,
    Long = 1,
    String = 2
}

/// <summary>Общие константы модуля CustomFields: имя БД-подключения, схема, ограничения, префикс кэша.</summary>
public static class CustomFieldsConstants
{
    public const string ConnectionStringName = "CustomFields";

    public const string Schema = "custom_fields";

    public const int MaxEntityTypeLength = 100;
    public const int MaxKeyLength = 100;
    public const int MaxLabelLength = 200;
    public const int MaxEntityIdLength = 200;
    public const int MaxOwnerServiceLength = 100;

    /// <summary>Префикс ключа кэша определений: <c>cf:def:{tenant}:{entityType}</c>.</summary>
    public const string DefinitionsCacheKeyPrefix = "cf:def:";

    /// <summary>TTL кэша определений.</summary>
    public static readonly TimeSpan DefinitionsCacheTtl = TimeSpan.FromMinutes(10);

    /// <summary>Базовый префикс REST-маршрутов модуля.</summary>
    public const string DefaultRoutePrefix = "/api/custom-fields";

    /// <summary>Строит ключ кэша определений для типа в скоупе тенанта.</summary>
    public static string DefinitionsCacheKey(Guid? tenantId, string entityType)
        => $"{DefinitionsCacheKeyPrefix}{(tenantId?.ToString() ?? "global")}:{entityType}";
}

/// <summary>Конвенции ключей типов сущностей — стабильный контракт <c>"{service}.{entity}"</c> (как в Tags).</summary>
public static class CustomFieldEntityTypes
{
    public static string Compose(string service, string entity) => $"{service}.{entity}";
}
