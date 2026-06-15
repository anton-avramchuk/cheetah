namespace Cheetah.Modules.Deals.Shared;

/// <summary>Общие константы модуля Deals: имя БД-подключения, схема и ограничения длин.</summary>
public static class DealsConstants
{
    public const string ConnectionStringName = "Deals";

    public const string Schema = "deals";

    public const int MaxTitleLength = 300;
    public const int MaxNameLength = 200;
    public const int MaxLostReasonLength = 1000;
    public const int MaxCurrencyLength = 3;

    /// <summary>Точность денежных колонок: numeric(18,2).</summary>
    public const string MoneyColumnType = "numeric(18,2)";
}

/// <summary>
/// Ключ полиморфной привязки сделки для других модулей (Activities/Notes/Calendar/Search).
/// Единая конвенция <c>(EntityType, EntityId)</c>, как в Tags.
/// </summary>
public static class DealEntityRefKeys
{
    public const string Deal = "crm.deal";
}
