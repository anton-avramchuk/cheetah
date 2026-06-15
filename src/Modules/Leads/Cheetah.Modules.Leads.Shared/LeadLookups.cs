namespace Cheetah.Modules.Leads.Shared;

/// <summary>
/// Стабильные коды известных статусов лида. Сами статусы — данные (справочник <c>LeadStatus</c>),
/// но эти коды — контракт: на них опираются доменные переходы и seed.
/// </summary>
public static class LeadStatusCodes
{
    public const string New = "New";
    public const string Working = "Working";
    public const string Qualified = "Qualified";
    public const string Converted = "Converted";
    public const string Disqualified = "Disqualified";
}

/// <summary>Стабильные коды известных источников лида (справочник <c>LeadSource</c>).</summary>
public static class LeadSourceCodes
{
    public const string Web = "Web";
    public const string Import = "Import";
    public const string Ads = "Ads";
    public const string Referral = "Referral";
    public const string Manual = "Manual";
    public const string Api = "Api";
}

/// <summary>
/// Фиксированные идентификаторы seed-строк справочников. Детерминированы: на них ссылаются доменные
/// инварианты (<c>LeadBase</c>), seed-конфигурации EF и тесты — поэтому одинаковы во всех средах.
/// </summary>
public static class LeadWellKnownIds
{
    // Статусы
    public static readonly Guid StatusNew = new("a1a1a1a1-0000-0000-0000-000000000001");
    public static readonly Guid StatusWorking = new("a1a1a1a1-0000-0000-0000-000000000002");
    public static readonly Guid StatusQualified = new("a1a1a1a1-0000-0000-0000-000000000003");
    public static readonly Guid StatusConverted = new("a1a1a1a1-0000-0000-0000-000000000004");
    public static readonly Guid StatusDisqualified = new("a1a1a1a1-0000-0000-0000-000000000005");

    // Источники
    public static readonly Guid SourceWeb = new("b2b2b2b2-0000-0000-0000-000000000001");
    public static readonly Guid SourceImport = new("b2b2b2b2-0000-0000-0000-000000000002");
    public static readonly Guid SourceAds = new("b2b2b2b2-0000-0000-0000-000000000003");
    public static readonly Guid SourceReferral = new("b2b2b2b2-0000-0000-0000-000000000004");
    public static readonly Guid SourceManual = new("b2b2b2b2-0000-0000-0000-000000000005");
    public static readonly Guid SourceApi = new("b2b2b2b2-0000-0000-0000-000000000006");
}
