namespace Cheetah.Modules.Leads.Shared;

/// <summary>Жизненный цикл лида (участвует в конечном автомате).</summary>
public enum LeadStatus
{
    New = 0,
    Working = 1,
    Qualified = 2,
    Converted = 3,
    Disqualified = 4
}

/// <summary>Источник лида.</summary>
public enum LeadSource
{
    Web = 0,
    Import = 1,
    Ads = 2,
    Referral = 3,
    Manual = 4,
    Api = 5
}
