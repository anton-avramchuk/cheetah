namespace Cheetah.Modules.Activities.Shared;

/// <summary>
/// Конвенция ключей полиморфной привязки активности к произвольной сущности CRM
/// (<c>EntityType</c> + <c>EntityId</c>) — общая с Tags/Notes/CustomFields. Значения не
/// ограничивают модуль: можно передать любой строковый <c>EntityType</c>; это лишь известные ключи.
/// </summary>
public static class EntityRefKeys
{
    public const string Deal = "crm.deal";
    public const string Customer = "crm.customer";
    public const string Contact = "crm.contact";
    public const string Lead = "crm.lead";
}
