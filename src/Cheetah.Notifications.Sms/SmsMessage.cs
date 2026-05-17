namespace Cheetah.Notifications.Sms;

/// <summary>
/// Сообщение SMS-канала. To — телефон в международном формате (+79991234567).
/// </summary>
public sealed record SmsMessage(string To, string Body);
