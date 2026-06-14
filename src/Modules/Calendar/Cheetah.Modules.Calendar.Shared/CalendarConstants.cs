namespace Cheetah.Modules.Calendar.Shared;

/// <summary>Общие константы модуля Calendar: имя БД-подключения и ограничения длин.</summary>
public static class CalendarConstants
{
    public const string ConnectionStringName = "Calendar";

    public const string Schema = "calendar";

    public const int MaxTitleLength = 256;
    public const int MaxDescriptionLength = 4000;
    public const int MaxLocationLength = 512;
    public const int MaxNameLength = 128;
    public const int MaxColorLength = 16;
    public const int MaxTimeZoneLength = 64;
    public const int MaxEntityTypeLength = 128;
    public const int MaxRRuleLength = 1024;
    public const int MaxStatusLength = 16;
    public const int MaxOccurrenceKeyLength = 32;
    public const int MaxReasonLength = 512;
}

/// <summary>
/// Ключи шаблонов уведомлений, которые публикует Calendar. Сам шаблон (subject/body)
/// живёт в модуле Notification; здесь — только ключ, общий для обеих сторон.
/// </summary>
public static class CalendarTemplates
{
    /// <summary>Напоминание о предстоящем событии.</summary>
    public const string Reminder = "calendar.reminder";

    /// <summary>Приглашение участника на событие.</summary>
    public const string Invite = "calendar.invite";
}

/// <summary>Категория уведомления Calendar — совпадает со строковыми значениями Notification.</summary>
public static class CalendarNotificationCategories
{
    public const string System = "System";
}
