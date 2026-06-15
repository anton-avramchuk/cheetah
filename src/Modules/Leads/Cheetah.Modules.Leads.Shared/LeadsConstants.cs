namespace Cheetah.Modules.Leads.Shared;

/// <summary>
/// Общие константы шаблонного модуля Leads: имя БД-подключения, ограничения длин, дефолтные имена
/// таблицы/схемы и префикс маршрутов. Используются абстрактными базами (<c>LeadConfigurationBase</c>,
/// <c>LeadEndpointsBase</c>) как значения по умолчанию, которые наследник может переопределить.
/// </summary>
public static class LeadsConstants
{
    public const string ConnectionStringName = "Leads";

    public const int MaxNameLength = 300;
    public const int MaxEmailLength = 320;
    public const int MaxPhoneLength = 40;
    public const int MaxReasonLength = 1000;

    public const string DefaultSchema = "leads";
    public const string DefaultTableName = "Leads";

    public const string DefaultRoutePrefix = "api/leads";
    public const string PublicRoutePrefix = "api/public/leads";
}
