namespace Cheetah.Core.DataAccess;

public static class CrmCommonDbProperties
{
    
    public static string DbTablePrefix { get; set; } = "Crm";

    /// <summary>
    /// Default value: null.
    /// </summary>
    public static string? DbSchema { get; set; } = null;
}