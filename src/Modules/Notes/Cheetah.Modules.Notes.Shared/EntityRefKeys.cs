namespace Cheetah.Modules.Notes.Shared;

/// <summary>
/// Конвенция ключей полиморфной привязки заметки к произвольной сущности CRM
/// (<c>EntityType</c> + <c>EntityId</c>) — общая с Tags/Activities/CustomFields. Значения не
/// ограничивают модуль: можно передать любой строковый <c>EntityType</c>; это лишь известные ключи.
/// </summary>
public static class EntityRefKeys
{
    public const string Deal = "crm.deal";
    public const string Customer = "crm.customer";
    public const string Contact = "crm.contact";
    public const string Lead = "crm.lead";
    public const string Invoice = "crm.invoice";
}
