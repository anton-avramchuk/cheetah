namespace Cheetah.Modules.Notification.Shared;

/// <summary>
/// Канал доставки. Строковые значения совпадают с <see cref="NotificationChannels"/>
/// и используются в контракте NotificationRequested (там — как string, чтобы DomainEvents
/// не зависел от Shared).
/// </summary>
public enum NotificationChannel
{
    Email,
    Sms,
    Push
}

/// <summary>Категория уведомления — определяет правила opt-out и unsubscribe.</summary>
public enum NotificationCategory
{
    /// <summary>Транзакционное (чек, статус заказа) — обычно нельзя отключить.</summary>
    Transactional,
    /// <summary>Одноразовый код — наивысший приоритет, отключению не подлежит.</summary>
    Otp,
    /// <summary>Маркетинг — требует явного opt-in, обязателен unsubscribe.</summary>
    Marketing,
    /// <summary>Системные оповещения.</summary>
    System
}

/// <summary>Статус доставки по конкретному каналу (одна попытка = один Dispatch).</summary>
public enum DispatchStatus
{
    /// <summary>Создан, событие каналу опубликовано, ждём подтверждения.</summary>
    Pending,
    /// <summary>Канал принял сообщение в шлюз.</summary>
    Sent,
    /// <summary>Подтверждена доставка (вебхуком провайдера).</summary>
    Delivered,
    /// <summary>Окончательная неудача (битый адрес, отказ шлюза, исчерпаны ретраи).</summary>
    Failed,
    /// <summary>Bounce/complaint из вебхука провайдера.</summary>
    Bounced,
    /// <summary>Подавлено политикой (opt-out, quiet hours, нет контакта).</summary>
    Suppressed
}
