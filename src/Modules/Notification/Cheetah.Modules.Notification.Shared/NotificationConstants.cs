namespace Cheetah.Modules.Notification.Shared;

/// <summary>Общие константы модуля Notification: имя БД-подключения и ограничения длин.</summary>
public static class NotificationConstants
{
    public const string ConnectionStringName = "Notification";

    public const int MaxTemplateKeyLength = 128;
    public const int MaxCategoryLength = 32;
    public const int MaxChannelLength = 16;
    public const int MaxContactValueLength = 256;
    public const int MaxErrorLength = 1024;
}

/// <summary>
/// Канонические строковые имена каналов. Совпадают с именами <see cref="NotificationChannel"/>.
/// Используются в payload событий (string) и при сопоставлении каналов с роутером.
/// </summary>
public static class NotificationChannels
{
    public const string Email = nameof(Email);
    public const string Sms = nameof(Sms);
    public const string Push = nameof(Push);
}
