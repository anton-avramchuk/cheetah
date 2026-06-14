namespace Cheetah.Modules.Notification.Contracts;

/// <summary>
/// Запрос на отправку уведомления через REST. В реальном потоке уведомления приходят
/// событиями (NotificationRequested) от модулей-продюсеров; этот эндпоинт — удобный
/// ручной триггер для интеграционной проверки конвейера.
/// </summary>
public sealed record RequestNotificationRequest(
    Guid RecipientUserId,
    string TemplateKey,
    string Category,
    IReadOnlyDictionary<string, string> Data,
    string? ForceChannel = null);
