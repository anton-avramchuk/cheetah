namespace Cheetah.Modules.Notification.Contracts;

/// <summary>Запись истории: одно уведомление с его попытками доставки по каналам.</summary>
public sealed record NotificationHistoryDto(
    Guid Id,
    Guid RecipientUserId,
    string TemplateKey,
    string Category,
    DateTimeOffset? CreatedAt,
    IReadOnlyList<NotificationDispatchDto> Dispatches);

/// <summary>Одна попытка доставки уведомления по конкретному каналу.</summary>
public sealed record NotificationDispatchDto(
    Guid Id,
    string Channel,
    string Status,
    string? Error,
    DateTimeOffset? UpdatedAt);
