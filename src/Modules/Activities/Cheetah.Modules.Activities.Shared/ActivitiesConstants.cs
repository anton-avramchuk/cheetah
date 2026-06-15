namespace Cheetah.Modules.Activities.Shared;

/// <summary>
/// Общие константы шаблонного модуля Activities: имя БД-подключения, ограничения длин,
/// дефолтные имена таблиц/схемы и префикс маршрутов. Используются абстрактными базами
/// (<c>ActivityConfigurationBase</c>, <c>ActivityEndpointsBase</c>) как значения по умолчанию,
/// которые наследник может переопределить.
/// </summary>
public static class ActivitiesConstants
{
    public const string ConnectionStringName = "Activities";

    public const int MaxTitleLength = 300;
    public const int MaxEntityTypeLength = 64;
    public const int MaxResultLength = 2000;
    public const int MaxChannelLength = 32;

    public const string DefaultSchema = "activities";
    public const string DefaultTableName = "Activities";
    public const string DefaultRemindersTableName = "ActivityReminders";

    public const string DefaultRoutePrefix = "api/activities";
}
