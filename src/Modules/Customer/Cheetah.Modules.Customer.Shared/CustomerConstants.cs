namespace Cheetah.Modules.Customer.Shared;

/// <summary>
/// Общие константы шаблонного модуля Customer: имя БД-подключения, ограничения длин,
/// дефолтные имена таблицы/схемы и префикс маршрутов. Используются абстрактными базами
/// (<c>CustomerConfigurationBase</c>, <c>CustomerEndpointsBase</c>) как значения по умолчанию,
/// которые наследник может переопределить.
/// </summary>
public static class CustomerConstants
{
    public const string ConnectionStringName = "Customer";

    public const int MaxNameLength = 256;
    public const int MaxEmailLength = 320;
    public const int MaxPhoneLength = 32;

    // Контактные лица клиента (отдельный агрегат со ссылкой CustomerId).
    public const int MaxFullNameLength = 256;

    public const string DefaultSchema = "customer";
    public const string DefaultTableName = "Customers";
    public const string DefaultContactsTableName = "Contacts";
    public const string DefaultPositionsTableName = "Positions";

    public const string DefaultRoutePrefix = "api/customers";

    /// <summary>Шаблон вложенного маршрута контактов: <c>api/customers/{customerId}/contacts</c>.</summary>
    public const string DefaultContactsRouteSuffix = "contacts";
}
