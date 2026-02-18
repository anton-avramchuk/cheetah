namespace Crm.Recruitment.Frontend;

internal static class RecruitmentCacheKeys
{
    private const string Prefix = "recruitment:";

    public const string Customers = $"{Prefix}customers";
    public const string Positions = $"{Prefix}positions";
    public const string StackItems = $"{Prefix}stack-items";
    public const string WorkFormats = $"{Prefix}work-formats";
    public const string VacancyStates = $"{Prefix}vacancy-states";
}
